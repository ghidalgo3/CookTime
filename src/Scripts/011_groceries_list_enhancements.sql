-- PostgreSQL Migration: Groceries List Enhancements
-- Adds selected ingredients tracking and quantity support for recipe lists

-- Selected ingredients junction table (tracks which ingredients user has checked off in a list)
CREATE TABLE IF NOT EXISTS cooktime.recipe_list_selected_ingredients (
    recipe_list_id uuid NOT NULL REFERENCES cooktime.recipe_lists(id) ON DELETE CASCADE,
    ingredient_id uuid NOT NULL REFERENCES cooktime.ingredients(id) ON DELETE CASCADE,
    PRIMARY KEY (recipe_list_id, ingredient_id)
);

CREATE INDEX IF NOT EXISTS idx_recipe_list_selected_ingredients_list_id 
ON cooktime.recipe_list_selected_ingredients(recipe_list_id);

-- Update get_recipe_list_with_recipes to include quantity and selectedIngredients
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
                            'recipe', cooktime.recipe_to_summary(r),
                            'quantity', rr.quantity
                        )
                    )
                    FROM cooktime.recipe_requirements rr
                    JOIN cooktime.recipes r ON r.id = rr.recipe_id
                    WHERE rr.recipe_list_id = rl.id
                ),
                '[]'::jsonb
            ),
            'selectedIngredients', COALESCE(
                (
                    SELECT jsonb_agg(si.ingredient_id)
                    FROM cooktime.recipe_list_selected_ingredients si
                    WHERE si.recipe_list_id = rl.id
                ),
                '[]'::jsonb
            )
        )
        FROM cooktime.recipe_lists rl
        WHERE rl.id = get_recipe_list_with_recipes.list_id
    );
END;
$$ LANGUAGE plpgsql;

-- Update recipe quantity in list
CREATE OR REPLACE FUNCTION cooktime.update_recipe_quantity_in_list(
    p_list_id uuid,
    p_recipe_id uuid,
    p_quantity double precision
)
RETURNS void AS $$
BEGIN
    UPDATE cooktime.recipe_requirements
    SET quantity = p_quantity
    WHERE recipe_list_id = p_list_id AND recipe_id = p_recipe_id;
    
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Recipe % not found in list %', p_recipe_id, p_list_id;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Toggle selected ingredient in list (add if not present, remove if present)
CREATE OR REPLACE FUNCTION cooktime.toggle_selected_ingredient(
    p_list_id uuid,
    p_ingredient_id uuid
)
RETURNS boolean AS $$
DECLARE
    was_selected boolean;
BEGIN
    -- Check if ingredient is currently selected
    DELETE FROM cooktime.recipe_list_selected_ingredients
    WHERE recipe_list_id = p_list_id AND ingredient_id = p_ingredient_id
    RETURNING true INTO was_selected;
    
    IF was_selected THEN
        -- Was selected, now unselected
        RETURN false;
    ELSE
        -- Was not selected, add it
        INSERT INTO cooktime.recipe_list_selected_ingredients (recipe_list_id, ingredient_id)
        VALUES (p_list_id, p_ingredient_id);
        RETURN true;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Clear all selected ingredients in list
CREATE OR REPLACE FUNCTION cooktime.clear_selected_ingredients(p_list_id uuid)
RETURNS integer AS $$
DECLARE
    deleted_count integer;
BEGIN
    DELETE FROM cooktime.recipe_list_selected_ingredients
    WHERE recipe_list_id = p_list_id;
    
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql;

-- Get aggregated ingredients for a recipe list (for grocery shopping)
-- Aggregates all ingredients from all recipes in the list, applying quantity multipliers
CREATE OR REPLACE FUNCTION cooktime.get_list_aggregated_ingredients(p_list_id uuid)
RETURNS jsonb AS $$
BEGIN
    RETURN COALESCE(
        (
            SELECT jsonb_agg(
                jsonb_build_object(
                    'ingredient', jsonb_build_object(
                        'id', agg.ingredient_id,
                        'name', agg.ingredient_name,
                        'isNew', false,
                        'densityKgPerL', agg.density
                    ),
                    'quantity', agg.total_quantity,
                    'unit', agg.unit,
                    'selected', EXISTS (
                        SELECT 1 FROM cooktime.recipe_list_selected_ingredients si
                        WHERE si.recipe_list_id = p_list_id AND si.ingredient_id = agg.ingredient_id
                    )
                )
                ORDER BY agg.ingredient_name
            )
            FROM (
                SELECT 
                    i.id AS ingredient_id,
                    i.name AS ingredient_name,
                    COALESCE(nf.density, 1.0) AS density,
                    ir.unit::text AS unit,
                    SUM(ir.quantity * rr.quantity) AS total_quantity
                FROM cooktime.recipe_requirements rr
                JOIN cooktime.recipes r ON r.id = rr.recipe_id
                JOIN cooktime.recipe_components rc ON rc.recipe_id = r.id
                JOIN cooktime.ingredient_requirements ir ON ir.recipe_component_id = rc.id
                JOIN cooktime.ingredients i ON i.id = ir.ingredient_id
                LEFT JOIN cooktime.nutrition_facts nf ON nf.id = i.nutrition_facts_id
                WHERE rr.recipe_list_id = p_list_id
                GROUP BY i.id, i.name, nf.density, ir.unit
            ) agg
        ),
        '[]'::jsonb
    );
END;
$$ LANGUAGE plpgsql;
