-- Migration: Recipe cook history and recommendations
-- Tracks when users cook recipes and recommends related recipes.
-- One cook event per user/recipe/day (idempotent logging).

CREATE TABLE IF NOT EXISTS cooktime.recipe_cook_events (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES cooktime.users(id),
    recipe_id uuid NOT NULL REFERENCES cooktime.recipes(id) ON DELETE CASCADE,
    cooked_at date NOT NULL DEFAULT current_date,
    created_date timestamptz NOT NULL DEFAULT now()
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_recipe_cook_events_user_recipe_cooked_at_unique
ON cooktime.recipe_cook_events(user_id, recipe_id, cooked_at);

CREATE INDEX IF NOT EXISTS idx_recipe_cook_events_user_recipe_cooked_at
ON cooktime.recipe_cook_events(user_id, recipe_id, cooked_at DESC);

CREATE INDEX IF NOT EXISTS idx_recipe_cook_events_user_cooked_at
ON cooktime.recipe_cook_events(user_id, cooked_at DESC);

CREATE INDEX IF NOT EXISTS idx_recipe_cook_events_recipe_id
ON cooktime.recipe_cook_events(recipe_id);

CREATE OR REPLACE FUNCTION cooktime.cook_event_to_json(e cooktime.recipe_cook_events)
RETURNS jsonb AS $$
BEGIN
    RETURN jsonb_build_object(
        'id', e.id,
        'userId', e.user_id,
        'recipeId', e.recipe_id,
        'cookedAt', e.cooked_at,
        'createdDate', e.created_date
    );
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION cooktime.create_recipe_cook_event(
    p_user_id uuid,
    p_recipe_id uuid,
    p_cooked_at date DEFAULT NULL
)
RETURNS jsonb AS $$
DECLARE
    new_event cooktime.recipe_cook_events;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM cooktime.recipes WHERE id = p_recipe_id) THEN
        RAISE EXCEPTION 'Recipe % not found', p_recipe_id;
    END IF;

    INSERT INTO cooktime.recipe_cook_events (user_id, recipe_id, cooked_at)
    VALUES (p_user_id, p_recipe_id, COALESCE(p_cooked_at, current_date))
    ON CONFLICT (user_id, recipe_id, cooked_at) DO UPDATE SET cooked_at = excluded.cooked_at
    RETURNING * INTO new_event;

    RETURN cooktime.cook_event_to_json(new_event);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION cooktime.get_recipe_cook_history(
    p_user_id uuid,
    p_recipe_id uuid
)
RETURNS jsonb AS $$
BEGIN
    RETURN COALESCE(
        (
            SELECT jsonb_agg(cooktime.cook_event_to_json(e) ORDER BY e.cooked_at DESC, e.created_date DESC)
            FROM cooktime.recipe_cook_events e
            WHERE e.user_id = p_user_id
              AND e.recipe_id = p_recipe_id
        ),
        '[]'::jsonb
    );
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION cooktime.update_recipe_cook_event(
    p_user_id uuid,
    p_event_id uuid,
    p_cooked_at date
)
RETURNS jsonb AS $$
DECLARE
    updated_event cooktime.recipe_cook_events;
BEGIN
    UPDATE cooktime.recipe_cook_events
    SET cooked_at = p_cooked_at
    WHERE id = p_event_id
      AND user_id = p_user_id
    RETURNING * INTO updated_event;

    IF updated_event.id IS NULL THEN
        RAISE EXCEPTION 'Cook event % not found', p_event_id;
    END IF;

    RETURN cooktime.cook_event_to_json(updated_event);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION cooktime.delete_recipe_cook_event(
    p_user_id uuid,
    p_event_id uuid
)
RETURNS boolean AS $$
DECLARE
    deleted_count integer;
BEGIN
    DELETE FROM cooktime.recipe_cook_events
    WHERE id = p_event_id
      AND user_id = p_user_id;

    GET DIAGNOSTICS deleted_count = ROW_COUNT;

    IF deleted_count = 0 THEN
        RAISE EXCEPTION 'Cook event % not found', p_event_id;
    END IF;

    RETURN true;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION cooktime.recommend_recipes(
    p_source_recipe_id uuid,
    p_user_id uuid DEFAULT NULL,
    p_limit integer DEFAULT 6
)
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
    candidate_ingredients AS (
        SELECT r.id AS recipe_id, ir.ingredient_id
        FROM cooktime.recipes r
        JOIN cooktime.recipe_components rc ON rc.recipe_id = r.id
        JOIN cooktime.ingredient_requirements ir ON ir.recipe_component_id = rc.id
        WHERE r.id <> p_source_recipe_id
          AND ir.ingredient_id IS NOT NULL
        GROUP BY r.id, ir.ingredient_id
    ),
    source_count AS (
        SELECT COUNT(*)::double precision AS ingredient_count
        FROM source_ingredients
    ),
    candidate_scores AS (
        SELECT
            r.id AS recipe_id,
            CASE
                WHEN (sc.ingredient_count + COUNT(ci.ingredient_id) - COUNT(si.ingredient_id)) = 0 THEN 0::double precision
                ELSE COUNT(si.ingredient_id)::double precision / (sc.ingredient_count + COUNT(ci.ingredient_id) - COUNT(si.ingredient_id))::double precision
            END AS ingredient_similarity,
            CASE WHEN p_user_id IS NOT NULL AND r.owner_id = p_user_id THEN 1::double precision ELSE 0::double precision END AS owned_by_user,
            CASE WHEN p_user_id IS NOT NULL AND EXISTS (
                SELECT 1
                FROM cooktime.recipe_lists rl
                JOIN cooktime.recipe_requirements rr ON rr.recipe_list_id = rl.id
                WHERE rl.owner_id = p_user_id
                  AND lower(rl.name) = 'favorites'
                  AND rr.recipe_id = r.id
            ) THEN 1::double precision ELSE 0::double precision END AS favorited_by_user,
            CASE
                WHEN p_user_id IS NULL THEN 0::double precision
                WHEN last_cooked.last_cooked_at IS NULL THEN 1::double precision
                WHEN current_date - last_cooked.last_cooked_at <= 7 THEN 0::double precision
                WHEN current_date - last_cooked.last_cooked_at <= 30 THEN 0.5::double precision
                ELSE 1::double precision
            END AS novelty
        FROM cooktime.recipes r
        CROSS JOIN source_count sc
        LEFT JOIN candidate_ingredients ci ON ci.recipe_id = r.id
        LEFT JOIN source_ingredients si ON si.ingredient_id = ci.ingredient_id
        LEFT JOIN LATERAL (
            SELECT MAX(e.cooked_at) AS last_cooked_at
            FROM cooktime.recipe_cook_events e
            WHERE p_user_id IS NOT NULL
              AND e.user_id = p_user_id
              AND e.recipe_id = r.id
        ) last_cooked ON true
        WHERE r.id <> p_source_recipe_id
        GROUP BY r.id, r.owner_id, sc.ingredient_count, last_cooked.last_cooked_at
    ),
    weighted_scores AS (
        SELECT
            cs.recipe_id,
            cs.ingredient_similarity,
            CASE WHEN p_user_id IS NULL THEN 0::double precision ELSE cs.owned_by_user * 0.15 END AS owned_contribution,
            CASE WHEN p_user_id IS NULL THEN 0::double precision ELSE cs.favorited_by_user * 0.15 END AS favorite_contribution,
            CASE WHEN p_user_id IS NULL THEN 0::double precision ELSE cs.novelty * 0.10 END AS novelty_contribution,
            CASE
                WHEN p_user_id IS NULL THEN cs.ingredient_similarity
                ELSE cs.ingredient_similarity * 0.60
                    + cs.owned_by_user * 0.15
                    + cs.favorited_by_user * 0.15
                    + cs.novelty * 0.10
            END AS total_score,
            cs.owned_by_user,
            cs.favorited_by_user,
            cs.novelty
        FROM candidate_scores cs
    )
    SELECT jsonb_build_object(
        'recipe', cooktime.recipe_to_summary(r),
        'score', ws.total_score,
        'scoreBreakdown', jsonb_build_object(
            'ingredientSimilarity', CASE WHEN p_user_id IS NULL THEN ws.ingredient_similarity ELSE ws.ingredient_similarity * 0.60 END,
            'ownedByUser', ws.owned_contribution,
            'favoritedByUser', ws.favorite_contribution,
            'novelty', ws.novelty_contribution,
            'dietMatch', 0
        ),
        'reasons', (
            SELECT jsonb_agg(reason)
            FROM (
                SELECT 'Similar ingredients' AS reason WHERE ws.ingredient_similarity > 0
                UNION ALL
                SELECT 'Your recipe' WHERE ws.owned_by_user > 0
                UNION ALL
                SELECT 'Favorite' WHERE ws.favorited_by_user > 0
                UNION ALL
                SELECT 'Not cooked recently' WHERE p_user_id IS NOT NULL AND ws.novelty > 0
            ) reasons
        )
    )
    FROM weighted_scores ws
    JOIN cooktime.recipes r ON r.id = ws.recipe_id
    WHERE ws.ingredient_similarity > 0
       OR (p_user_id IS NOT NULL AND (ws.owned_by_user > 0 OR ws.favorited_by_user > 0))
    ORDER BY
        CASE WHEN ws.ingredient_similarity > 0 THEN 0 ELSE 1 END,
        ws.total_score DESC,
        r.created_date DESC
    LIMIT GREATEST(0, p_limit);
END;
$$ LANGUAGE plpgsql;
