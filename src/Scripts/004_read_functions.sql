-- PostgreSQL Migration: Read Stored Procedures
-- Procedures for querying data and returning JSON

-- Get recipe with full details
CREATE OR REPLACE FUNCTION cooktime.get_recipe_with_details(recipe_id uuid)
RETURNS jsonb AS $$
BEGIN
    RETURN (
        SELECT jsonb_build_object(
            'id', r.id,
            'name', r.name,
            'description', r.description,
            'owner', CASE 
                WHEN r.owner_id IS NOT NULL THEN
                    jsonb_build_object('id', r.owner_id, 'userName', '')
                ELSE NULL
            END,
            'cooktimeMinutes', r.cooking_minutes,
            'caloriesPerServing', r.calories,
            'servingsProduced', r.servings,
            'source', r.source,
            'staticImage', (
                SELECT i.static_image_name
                FROM cooktime.images i
                WHERE i.recipe_id = r.id
                ORDER BY i.uploaded_date DESC
                LIMIT 1
            ),
            'recipeComponents', COALESCE(
                (
                    SELECT jsonb_agg(
                        jsonb_build_object(
                            'id', rc.id,
                            'name', rc.name,
                            'position', rc.position,
                            'steps', rc.steps,
                            'ingredients', COALESCE(
                                (
                                    SELECT jsonb_agg(
                                        jsonb_build_object(
                                            'id', ir.id,
                                            'ingredient', jsonb_build_object(
                                                'id', ing.id,
                                                'name', ing.name,
                                                'isNew', false,
!
                                                'densityKgPerL', null
                                            ),
                                            'text', ir.description,
                                            'unit', ir.unit::text,
                                            'quantity', ir.quantity,
                                            'position', ir.position
                                        )
                                        ORDER BY ir.position
                                    )
                                    FROM cooktime.ingredient_requirements ir
                                    LEFT JOIN cooktime.ingredients ing ON ing.id = ir.ingredient_id
                                    WHERE ir.recipe_component_id = rc.id
                                ),
                                '[]'::jsonb
                            )
                        )
                        ORDER BY rc.position
                    )
                    FROM cooktime.recipe_components rc
                    WHERE rc.recipe_id = r.id
                ),
                '[]'::jsonb
            ),
            'categories', COALESCE(
                (
                    SELECT jsonb_agg(
                        jsonb_build_object(
                            'id', c.id,
                            'name', c.name
                        )
                    )
                    FROM cooktime.categories c
                    JOIN cooktime.category_recipe cr ON cr.category_id = c.id
                    WHERE cr.recipe_id = r.id
                ),
                '[]'::jsonb
            ),
            'reviewCount', COALESCE((
                SELECT COUNT(*)::int
                FROM cooktime.reviews rv
                WHERE rv.recipe_id = r.id
            ), 0),
            'averageReviews', COALESCE((
                SELECT AVG(rv.rating)::double precision
                FROM cooktime.reviews rv
                WHERE rv.recipe_id = r.id
            ), 0)
        )
        FROM cooktime.recipes r
        WHERE r.id = get_recipe_with_details.recipe_id
    );
END;
$$ LANGUAGE plpgsql;

-- Unified search for recipes by name, description, category, or ingredient
CREATE OR REPLACE FUNCTION cooktime.search_recipes(
    search_term text,
    page_size integer DEFAULT 50,
    page_number integer DEFAULT 1
)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT DISTINCT ON (r.id) cooktime.recipe_to_summary(r)
    FROM cooktime.recipes r
    LEFT JOIN cooktime.category_recipe cr ON cr.recipe_id = r.id
    LEFT JOIN cooktime.categories c ON c.id = cr.category_id
    LEFT JOIN cooktime.recipe_components rc ON rc.recipe_id = r.id
    LEFT JOIN cooktime.ingredient_requirements ir ON ir.recipe_component_id = rc.id
    LEFT JOIN cooktime.ingredients i ON i.id = ir.ingredient_id
    WHERE 
        -- Match recipe name/description via full-text search
        r.search_vector @@ plainto_tsquery('english', search_term)
        -- Or match category name
        OR c.name ILIKE '%' || search_term || '%'
        -- Or match ingredient name
        OR i.name ILIKE '%' || search_term || '%'
    ORDER BY r.id, ts_rank(r.search_vector, plainto_tsquery('english', search_term)) DESC
    LIMIT page_size
    OFFSET (page_number - 1) * page_size;
END;
$$ LANGUAGE plpgsql;

-- Get all recipes (with pagination)
CREATE OR REPLACE FUNCTION cooktime.get_recipes(
    page_size integer DEFAULT 50,
    page_number integer DEFAULT 1
)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT jsonb_build_object(
        'id', r.id,
        'name', r.name,
        'description', r.description,
        'cookingMinutes', r.cooking_minutes,
        'servings', r.servings,
        'calories', r.calories,
        'createdDate', r.created_date
    )
    FROM cooktime.recipes r
    ORDER BY r.created_date DESC
    LIMIT page_size
    OFFSET (page_number - 1) * page_size;
END;
$$ LANGUAGE plpgsql;

-- Get user's recipe lists with optional name filter
CREATE OR REPLACE FUNCTION cooktime.get_user_recipe_lists(user_id uuid, name_filter text DEFAULT NULL)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT jsonb_build_object(
        'id', rl.id,
        'name', rl.name,
        'description', rl.description,
        'creationDate', rl.creation_date,
        'isPublic', rl.is_public,
        'recipeCount', (
            SELECT COUNT(*)
            FROM cooktime.recipe_requirements rr
            WHERE rr.recipe_list_id = rl.id
        )
    )
    FROM cooktime.recipe_lists rl
    WHERE rl.owner_id = get_user_recipe_lists.user_id
      AND (name_filter IS NULL OR rl.name ILIKE '%' || name_filter || '%')
    ORDER BY rl.creation_date DESC;
END;
$$ LANGUAGE plpgsql;

-- Get recipe list with recipes
CREATE OR REPLACE FUNCTION cooktime.get_recipe_list_with_recipes(list_id uuid)
RETURNS jsonb AS $$
BEGIN
    RETURN (
        SELECT jsonb_build_object(
            'id', rl.id,
            'name', rl.name,
            'description', rl.description,
            'creationDate', rl.creation_date,
            'isPublic', rl.is_public,
            'ownerId', rl.owner_id,
            'recipes', COALESCE(
                (
                    SELECT jsonb_agg(cooktime.recipe_to_summary(r))
                    FROM cooktime.recipe_requirements rr
                    JOIN cooktime.recipes r ON r.id = rr.recipe_id
                    WHERE rr.recipe_list_id = rl.id
                ),
                '[]'::jsonb
            )
        )
        FROM cooktime.recipe_lists rl
        WHERE rl.id = get_recipe_list_with_recipes.list_id
    );
END;
$$ LANGUAGE plpgsql;

-- Get recipe images
CREATE OR REPLACE FUNCTION cooktime.get_recipe_images(recipe_id uuid)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT jsonb_build_object(
        'id', i.id,
        'storageUrl', i.storage_url,
        'uploadedDate', i.uploaded_date,
        'staticImageName', i.static_image_name
    )
    FROM cooktime.images i
    WHERE i.recipe_id = get_recipe_images.recipe_id
    ORDER BY i.uploaded_date DESC;
END;
$$ LANGUAGE plpgsql;

-- Get ingredient by ID with nutrition facts
CREATE OR REPLACE FUNCTION cooktime.get_ingredient(ingredient_id uuid)
RETURNS jsonb AS $$
BEGIN
    RETURN (
        SELECT jsonb_build_object(
            'id', i.id,
            'name', i.name,
            'defaultServingSizeUnit', i.default_serving_size_unit,
            'nutritionFacts', CASE 
                WHEN i.nutrition_facts_id IS NOT NULL THEN
                    (
                        SELECT jsonb_build_object(
                            'id', nf.id,
                            'sourceIds', nf.source_ids,
                            'names', nf.names,
                            'unitMass', nf.unit_mass,
                            'density', nf.density,
                            'nutritionData', nf.nutrition_data
                        )
                        FROM cooktime.nutrition_facts nf
                        WHERE nf.id = i.nutrition_facts_id
                    )
                ELSE NULL
            END
        )
        FROM cooktime.ingredients i
        WHERE i.id = get_ingredient.ingredient_id
    );
END;
$$ LANGUAGE plpgsql;

-- Search ingredients by name (fuzzy matching with trigrams)
CREATE OR REPLACE FUNCTION cooktime.search_ingredients(search_term text)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT jsonb_build_object(
        'id', i.id,
        'name', i.name,
        'defaultServingSizeUnit', i.default_serving_size_unit
    )
    FROM cooktime.ingredients i
    WHERE similarity(i.name, search_term) > 0.3
    ORDER BY similarity(i.name, search_term) DESC
    LIMIT 20;
END;
$$ LANGUAGE plpgsql;

-- Get recipe reviews
CREATE OR REPLACE FUNCTION cooktime.get_recipe_reviews(recipe_id uuid)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT jsonb_build_object(
        'id', rev.id,
        'rating', rev.rating,
        'comment', rev.comment,
        'createdDate', rev.created_date,
        'lastModifiedDate', rev.last_modified_date,
        'ownerId', rev.owner_id
    )
    FROM cooktime.reviews rev
    WHERE rev.recipe_id = get_recipe_reviews.recipe_id
    ORDER BY rev.created_date DESC;
END;
$$ LANGUAGE plpgsql;

-- Map a recipe to a summary JSON object
CREATE OR REPLACE FUNCTION cooktime.recipe_to_summary(r cooktime.recipes)
RETURNS jsonb AS $$
BEGIN
    RETURN jsonb_build_object(
        'id', r.id,
        'name', r.name,
        'images', COALESCE((
            SELECT jsonb_agg(jsonb_build_object('id', i.id, 'name', i.static_image_name))
            FROM cooktime.images i
            WHERE i.recipe_id = r.id
        ), '[]'::jsonb),
        'categories', COALESCE((
            SELECT jsonb_agg(c.name)
            FROM cooktime.category_recipe cr
            JOIN cooktime.categories c ON c.id = cr.category_id
            WHERE cr.recipe_id = r.id
        ), '[]'::jsonb),
        'averageReviews', COALESCE((
            SELECT AVG(rv.rating)::double precision
            FROM cooktime.reviews rv
            WHERE rv.recipe_id = r.id
        ), 0),
        'reviewCount', COALESCE((
            SELECT COUNT(*)
            FROM cooktime.reviews rv
            WHERE rv.recipe_id = r.id
        ), 0)
    );
END;
$$ LANGUAGE plpgsql;

-- Get all categories
CREATE OR REPLACE FUNCTION cooktime.get_categories()
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT jsonb_build_object(
        'id', c.id,
        'name', c.name,
        'recipeCount', (
            SELECT COUNT(*)
            FROM cooktime.category_recipe cr
            WHERE cr.category_id = c.id
        )
    )
    FROM cooktime.categories c
    ORDER BY c.name;
END;
$$ LANGUAGE plpgsql;

-- Get recipe summaries by user ID (owner)
CREATE OR REPLACE FUNCTION cooktime.get_recipes_by_user(
    user_id uuid,
    page_size integer DEFAULT 50,
    page_number integer DEFAULT 1
)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT cooktime.recipe_to_summary(r)
    FROM cooktime.recipes r
    WHERE r.owner_id = get_recipes_by_user.user_id
    ORDER BY r.created_date DESC
    LIMIT page_size
    OFFSET (page_number - 1) * page_size;
END;
$$ LANGUAGE plpgsql;
