-- PostgreSQL Migration: Unified Ingredient Admin View
-- Combines get_ingredients_for_admin and get_normalized_ingredients into a single view

-- Function to get all ingredients with all admin fields including usage count
CREATE OR REPLACE FUNCTION cooktime.get_ingredients_unified()
RETURNS jsonb AS $$
BEGIN
    RETURN (
        SELECT COALESCE(jsonb_agg(
            jsonb_build_object(
                'ingredientId', i.id,
                'ingredientNames', i.name,
                'expectedUnitMass', COALESCE(i.expected_unit_mass_kg, 0.1)::text,
                'ndbNumber', COALESCE((nf.source_ids->>'ndbNumber')::integer, 0),
                'gtinUpc', COALESCE(nf.source_ids->>'gtinUpc', ''),
                'countRegex', COALESCE(nf.count_regex, ''),
                'nutritionDescription', COALESCE(nf.nutrition_data->>'description', ''),
                'usage', COALESCE(usage_counts.usage_count, 0),
                'hasNutrition', (i.nutrition_facts_id IS NOT NULL)
            )
            ORDER BY i.name
        ), '[]'::jsonb)
        FROM cooktime.ingredients i
        LEFT JOIN cooktime.nutrition_facts nf ON nf.id = i.nutrition_facts_id
        LEFT JOIN (
            SELECT ingredient_id, COUNT(*)::bigint as usage_count
            FROM cooktime.ingredient_requirements
            GROUP BY ingredient_id
        ) usage_counts ON usage_counts.ingredient_id = i.id
    );
END;
$$ LANGUAGE plpgsql;
