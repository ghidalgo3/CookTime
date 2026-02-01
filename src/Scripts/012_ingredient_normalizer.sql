-- PostgreSQL Migration: Ingredient Normalizer
-- Adds functions to merge duplicate ingredients and delete unused ingredients

-- Function to get all ingredients that appear in at least one recipe for normalization
CREATE OR REPLACE FUNCTION cooktime.get_normalized_ingredients()
RETURNS jsonb AS $$
BEGIN
    RETURN (
        WITH ingredient_usage AS (
            SELECT 
                i.id,
                i.name,
                COUNT(ir.id) as usage_count,
                (i.nutrition_facts_id IS NOT NULL) as has_nutrition
            FROM cooktime.ingredients i
            LEFT JOIN cooktime.ingredient_requirements ir ON ir.ingredient_id = i.id
            GROUP BY i.id, i.name, i.nutrition_facts_id
            HAVING COUNT(ir.id) > 0
        )
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'replacedId', iu.id,
                'name', iu.name,
                'usage', iu.usage_count,
                'hasNutrition', iu.has_nutrition,
                'keptId', '00000000-0000-0000-0000-000000000000'::text
            )
            ORDER BY iu.name
        ), '[]'::jsonb)
        FROM ingredient_usage iu
    );
END;
$$ LANGUAGE plpgsql;

-- Function to merge two ingredients (replace fromIngredientId with toIngredientId)
-- This is a transaction-safe operation that:
-- 1. Validates both ingredients exist
-- 2. Checks that they are different
-- 3. Updates all ingredient_requirements from fromIngredientId to toIngredientId
-- 4. Deletes the fromIngredientId ingredient
CREATE OR REPLACE FUNCTION cooktime.merge_ingredients(
    p_from_ingredient_id uuid,
    p_to_ingredient_id uuid
)
RETURNS boolean AS $$
DECLARE
    v_from_exists boolean;
    v_to_exists boolean;
BEGIN
    -- Check if from ingredient exists
    SELECT EXISTS(SELECT 1 FROM cooktime.ingredients WHERE id = p_from_ingredient_id)
    INTO v_from_exists;
    
    IF NOT v_from_exists THEN
        RAISE EXCEPTION 'From ingredient with ID % does not exist', p_from_ingredient_id;
    END IF;
    
    -- Check if to ingredient exists
    SELECT EXISTS(SELECT 1 FROM cooktime.ingredients WHERE id = p_to_ingredient_id)
    INTO v_to_exists;
    
    IF NOT v_to_exists THEN
        RAISE EXCEPTION 'To ingredient with ID % does not exist', p_to_ingredient_id;
    END IF;
    
    -- Check if they are the same ingredient
    IF p_from_ingredient_id = p_to_ingredient_id THEN
        RAISE EXCEPTION 'Cannot merge an ingredient with itself';
    END IF;
    
    -- Update all ingredient requirements pointing to fromIngredientId to toIngredientId
    UPDATE cooktime.ingredient_requirements
    SET ingredient_id = p_to_ingredient_id
    WHERE ingredient_id = p_from_ingredient_id;
    
    -- Delete the from ingredient
    DELETE FROM cooktime.ingredients
    WHERE id = p_from_ingredient_id;
    
    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

-- Function to delete an ingredient (only if it has no references)
CREATE OR REPLACE FUNCTION cooktime.delete_ingredient(
    p_ingredient_id uuid
)
RETURNS boolean AS $$
DECLARE
    v_exists boolean;
    v_in_use boolean;
BEGIN
    -- Check if ingredient exists
    SELECT EXISTS(SELECT 1 FROM cooktime.ingredients WHERE id = p_ingredient_id)
    INTO v_exists;
    
    IF NOT v_exists THEN
        RAISE EXCEPTION 'Ingredient with ID % does not exist', p_ingredient_id;
    END IF;
    
    -- Check if ingredient is in use
    SELECT EXISTS(
        SELECT 1 FROM cooktime.ingredient_requirements
        WHERE ingredient_id = p_ingredient_id
    )
    INTO v_in_use;
    
    IF v_in_use THEN
        RAISE EXCEPTION 'Cannot delete ingredient % because it is used in recipes', p_ingredient_id;
    END IF;
    
    -- Delete the ingredient
    DELETE FROM cooktime.ingredients
    WHERE id = p_ingredient_id;
    
    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;
