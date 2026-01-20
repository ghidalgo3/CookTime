-- Migration: Update get_recipes to use recipe_to_summary for consistent output with images
-- This ensures that all recipe listings include images

CREATE OR REPLACE FUNCTION cooktime.get_recipes(
    page_size integer DEFAULT 50,
    page_number integer DEFAULT 1
)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT cooktime.recipe_to_summary(r)
    FROM cooktime.recipes r
    ORDER BY r.created_date DESC
    LIMIT page_size
    OFFSET (page_number - 1) * page_size;
END;
$$ LANGUAGE plpgsql;
