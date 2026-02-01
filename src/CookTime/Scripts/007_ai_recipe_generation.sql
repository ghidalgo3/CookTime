-- Migration: Add confidence score to search_ingredients and add batch search function
-- For AI recipe generation feature

-- Update search_ingredients to include confidence score
CREATE OR REPLACE FUNCTION cooktime.search_ingredients(search_term text)
RETURNS jsonb AS $$
BEGIN
    RETURN COALESCE(
        (
            SELECT jsonb_agg(row_to_json(sub))
            FROM (
                SELECT i.id, i.name, similarity(i.name, search_term) AS confidence
                FROM cooktime.ingredients i
                WHERE i.name ILIKE '%' || search_term || '%'
                   OR similarity(i.name, search_term) > 0.2
                ORDER BY similarity(i.name, search_term) DESC
                LIMIT 20
            ) sub
        ),
        '[]'::jsonb
    );
END;
$$ LANGUAGE plpgsql;

-- Batch search ingredients by multiple names (for AI recipe generation)
-- Returns a JSON object where keys are the original search terms and values are arrays of matches
CREATE OR REPLACE FUNCTION cooktime.search_ingredients_batch(search_terms text[])
RETURNS jsonb AS $$
BEGIN
    RETURN COALESCE(
        (
            SELECT jsonb_object_agg(term, matches)
            FROM (
                SELECT 
                    term,
                    COALESCE(
                        (
                            SELECT jsonb_agg(
                                jsonb_build_object(
                                    'id', i.id,
                                    'name', i.name,
                                    'confidence', similarity(i.name, term)
                                )
                                ORDER BY similarity(i.name, term) DESC
                            )
                            FROM cooktime.ingredients i
                            WHERE i.name ILIKE '%' || term || '%'
                               OR similarity(i.name, term) > 0.2
                            LIMIT 10
                        ),
                        '[]'::jsonb
                    ) AS matches
                FROM UNNEST(search_terms) AS term
            ) sub
        ),
        '{}'::jsonb
    );
END;
$$ LANGUAGE plpgsql;
