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
            'cookingMinutes', r.cooking_minutes,
            'servings', r.servings,
            'prepMinutes', r.prep_minutes,
            'calories', r.calories,
            'source', r.source,
            'createdDate', r.created_date,
            'lastModifiedDate', r.last_modified_date,
            'ownerId', r.owner_id,
            'components', COALESCE(
                (
                    SELECT jsonb_agg(
                        jsonb_build_object(
                            'id', rc.id,
                            'name', rc.name,
                            'position', rc.position,
                            'steps', COALESCE(
                                (
                                    SELECT jsonb_agg(
                                        jsonb_build_object(
                                            'id', rs.id,
                                            'instruction', rs.instruction
                                        )
                                        ORDER BY rs.id
                                    )
                                    FROM cooktime.recipe_steps rs
                                    WHERE rs.recipe_component_id = rc.id
                                ),
                                '[]'::jsonb
                            ),
                            'ingredients', COALESCE(
                                (
                                    SELECT jsonb_agg(
                                        jsonb_build_object(
                                            'id', ir.id,
                                            'ingredientId', ir.ingredient_id,
                                            'unit', ir.unit::text,
                                            'quantity', ir.quantity,
                                            'position', ir.position,
                                            'description', ir.description
                                        )
                                        ORDER BY ir.position
                                    )
                                    FROM cooktime.ingredient_requirements ir
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
            )
        )
        FROM cooktime.recipes r
        WHERE r.id = get_recipe_with_details.recipe_id
    );
END;
$$ LANGUAGE plpgsql;

-- Search recipes by name using full-text search
CREATE OR REPLACE FUNCTION cooktime.search_recipes_by_name(search_term text)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT jsonb_build_object(
        'id', r.id,
        'name', r.name,
        'description', r.description,
        'cookingMinutes', r.cooking_minutes,
        'servings', r.servings,
        'calories', r.calories
    )
    FROM cooktime.recipes r
    WHERE r.search_vector @@ plainto_tsquery('english', search_term)
    ORDER BY ts_rank(r.search_vector, plainto_tsquery('english', search_term)) DESC;
END;
$$ LANGUAGE plpgsql;

-- Search recipes by ingredient
CREATE OR REPLACE FUNCTION cooktime.search_recipes_by_ingredient(ingredient_id uuid)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT DISTINCT jsonb_build_object(
        'id', r.id,
        'name', r.name,
        'description', r.description,
        'cookingMinutes', r.cooking_minutes,
        'servings', r.servings,
        'calories', r.calories
    )
    FROM cooktime.recipes r
    JOIN cooktime.recipe_components rc ON rc.recipe_id = r.id
    JOIN cooktime.ingredient_requirements ir ON ir.recipe_component_id = rc.id
    WHERE ir.ingredient_id = search_recipes_by_ingredient.ingredient_id
    ORDER BY r.name;
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

-- Get user's recipe lists
CREATE OR REPLACE FUNCTION cooktime.get_user_recipe_lists(user_id text)
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
                    SELECT jsonb_agg(
                        jsonb_build_object(
                            'recipeId', r.id,
                            'name', r.name,
                            'description', r.description,
                            'quantity', rr.quantity,
                            'cookingMinutes', r.cooking_minutes,
                            'servings', r.servings
                        )
                    )
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
