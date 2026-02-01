-- Migration: Add cooktimeMinutes to recipe_to_summary function
-- This allows recipe cards to display cook time information

CREATE OR REPLACE FUNCTION cooktime.recipe_to_summary(r cooktime.recipes)
RETURNS jsonb AS $$
BEGIN
    RETURN jsonb_build_object(
        'id', r.id,
        'name', r.name,
        'images', COALESCE((
            SELECT jsonb_agg(jsonb_build_object('id', i.id, 'url', i.storage_url))
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
        ), 0),
        'cooktimeMinutes', r.cooking_minutes
    );
END;
$$ LANGUAGE plpgsql;
