-- Migration: Recommendation primitives
-- The recommendation algorithm now lives in the application layer. The database
-- only exposes primitives (set-membership lookups backed by indexes); scoring,
-- weighting, the minimum-overlap gate, and ranking happen in C#.
-- This replaces the monolithic cooktime.recommend_recipes function.

DROP FUNCTION IF EXISTS cooktime.recommend_recipes(uuid, uuid, integer);

-- Candidate recipes that share at least one ingredient with the source recipe,
-- each with its full set of ingredient IDs. The application computes Jaccard
-- similarity and applies the minimum-overlap gate.
CREATE OR REPLACE FUNCTION cooktime.get_candidate_ingredient_sets(p_source_recipe_id uuid)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    WITH source_ingredients AS (
        SELECT DISTINCT ir.ingredient_id
        FROM cooktime.recipe_components rc
        JOIN cooktime.ingredient_requirements ir ON ir.recipe_component_id = rc.id
        WHERE rc.recipe_id = p_source_recipe_id
          AND ir.ingredient_id IS NOT NULL
    ),
    candidate_recipes AS (
        SELECT DISTINCT r.id AS recipe_id
        FROM cooktime.recipes r
        JOIN cooktime.recipe_components rc ON rc.recipe_id = r.id
        JOIN cooktime.ingredient_requirements ir ON ir.recipe_component_id = rc.id
        WHERE r.id <> p_source_recipe_id
          AND ir.ingredient_id IN (SELECT ingredient_id FROM source_ingredients)
    )
    SELECT jsonb_build_object(
        'recipeId', cr.recipe_id,
        'ingredientIds', COALESCE((
            SELECT jsonb_agg(DISTINCT ir.ingredient_id)
            FROM cooktime.recipe_components rc
            JOIN cooktime.ingredient_requirements ir ON ir.recipe_component_id = rc.id
            WHERE rc.recipe_id = cr.recipe_id
              AND ir.ingredient_id IS NOT NULL
        ), '[]'::jsonb)
    )
    FROM candidate_recipes cr;
END;
$$ LANGUAGE plpgsql;

-- Recipe IDs in the user's "favorites" list.
CREATE OR REPLACE FUNCTION cooktime.get_user_favorite_recipe_ids(p_user_id uuid)
RETURNS jsonb AS $$
BEGIN
    RETURN COALESCE(
        (
            SELECT jsonb_agg(rr.recipe_id)
            FROM cooktime.recipe_lists rl
            JOIN cooktime.recipe_requirements rr ON rr.recipe_list_id = rl.id
            WHERE rl.owner_id = p_user_id
              AND lower(rl.name) = 'favorites'
        ),
        '[]'::jsonb
    );
END;
$$ LANGUAGE plpgsql;

-- The most recent cook date per recipe for the user (drives novelty scoring).
CREATE OR REPLACE FUNCTION cooktime.get_user_last_cooked(p_user_id uuid)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT jsonb_build_object(
        'recipeId', e.recipe_id,
        'lastCookedAt', MAX(e.cooked_at)
    )
    FROM cooktime.recipe_cook_events e
    WHERE e.user_id = p_user_id
    GROUP BY e.recipe_id;
END;
$$ LANGUAGE plpgsql;

-- Recipe summaries for the final ranked set of recommendation winners.
CREATE OR REPLACE FUNCTION cooktime.get_recipe_summaries(p_recipe_ids uuid[])
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT cooktime.recipe_to_summary(r)
    FROM cooktime.recipes r
    WHERE r.id = ANY(p_recipe_ids);
END;
$$ LANGUAGE plpgsql;
