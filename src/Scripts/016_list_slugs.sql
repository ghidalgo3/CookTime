-- PostgreSQL Migration: List Slugs
-- Adds slug column to recipe_lists for user-friendly URLs

-- Add slug column to recipe_lists
ALTER TABLE cooktime.recipe_lists 
ADD COLUMN IF NOT EXISTS slug text;

-- Function to generate slug from name and id
CREATE OR REPLACE FUNCTION cooktime.generate_list_slug(p_name text, p_id uuid)
RETURNS text AS $$
BEGIN
    -- Convert name to lowercase, replace spaces with hyphens, remove special chars, append first 8 chars of uuid
    RETURN LOWER(REGEXP_REPLACE(REGEXP_REPLACE(p_name, '[^a-zA-Z0-9\s-]', '', 'g'), '\s+', '-', 'g')) 
           || '-' 
           || LEFT(REPLACE(p_id::text, '-', ''), 8);
END;
$$ LANGUAGE plpgsql;

-- Populate slugs for existing lists
UPDATE cooktime.recipe_lists
SET slug = cooktime.generate_list_slug(name, id)
WHERE slug IS NULL;

-- Make slug NOT NULL and add unique constraint
ALTER TABLE cooktime.recipe_lists 
ALTER COLUMN slug SET NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS idx_recipe_lists_slug_unique 
ON cooktime.recipe_lists(slug);

-- Create trigger to auto-generate slug on insert
CREATE OR REPLACE FUNCTION cooktime.recipe_lists_generate_slug()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.slug IS NULL THEN
        NEW.slug := cooktime.generate_list_slug(NEW.name, NEW.id);
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_recipe_lists_generate_slug ON cooktime.recipe_lists;
CREATE TRIGGER trg_recipe_lists_generate_slug
BEFORE INSERT ON cooktime.recipe_lists
FOR EACH ROW
EXECUTE FUNCTION cooktime.recipe_lists_generate_slug();

-- Drop and recreate update_recipe_list function to regenerate slug when name changes
-- (DROP is needed because PostgreSQL doesn't allow changing parameter names with CREATE OR REPLACE)
DROP FUNCTION IF EXISTS cooktime.update_recipe_list(uuid, uuid, text, text, boolean);
CREATE OR REPLACE FUNCTION cooktime.update_recipe_list(
    p_user_id uuid,
    p_list_id uuid,
    p_name text DEFAULT NULL,
    p_description text DEFAULT NULL,
    p_is_public boolean DEFAULT NULL
)
RETURNS jsonb AS $$
DECLARE
    list_owner uuid;
    updated_list jsonb;
    new_slug text;
BEGIN
    -- Get the owner of the list
    SELECT owner_id INTO list_owner
    FROM cooktime.recipe_lists
    WHERE id = p_list_id;
    
    -- Check if list exists
    IF list_owner IS NULL THEN
        RAISE EXCEPTION 'List % not found', p_list_id;
    END IF;
    
    -- Verify ownership
    IF list_owner != p_user_id THEN
        RAISE EXCEPTION 'User % does not own list %', p_user_id, p_list_id;
    END IF;
    
    -- If name is being updated, regenerate slug
    IF p_name IS NOT NULL THEN
        new_slug := cooktime.generate_list_slug(p_name, p_list_id);
    END IF;
    
    -- Update only the fields that are not null
    UPDATE cooktime.recipe_lists
    SET 
        name = COALESCE(p_name, name),
        description = COALESCE(p_description, description),
        is_public = COALESCE(p_is_public, is_public),
        slug = COALESCE(new_slug, slug)
    WHERE id = p_list_id;
    
    -- Return the updated list
    SELECT jsonb_build_object(
        'id', rl.id,
        'name', rl.name,
        'slug', rl.slug,
        'description', rl.description,
        'creationDate', rl.creation_date,
        'isPublic', rl.is_public,
        'recipeCount', (SELECT COUNT(*) FROM cooktime.recipe_requirements rr WHERE rr.recipe_list_id = rl.id)
    )
    INTO updated_list
    FROM cooktime.recipe_lists rl
    WHERE rl.id = p_list_id;
    
    RETURN updated_list;
END;
$$ LANGUAGE plpgsql;

-- Drop and recreate get_user_recipe_lists to return slug
-- (DROP is needed because PostgreSQL doesn't allow changing parameter names with CREATE OR REPLACE)
DROP FUNCTION IF EXISTS cooktime.get_user_recipe_lists(uuid, text);
CREATE OR REPLACE FUNCTION cooktime.get_user_recipe_lists(p_user_id uuid, p_filter text DEFAULT NULL)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT jsonb_build_object(
        'id', rl.id,
        'name', rl.name,
        'slug', rl.slug,
        'description', rl.description,
        'creationDate', rl.creation_date,
        'isPublic', rl.is_public,
        'recipeCount', (SELECT COUNT(*) FROM cooktime.recipe_requirements rr WHERE rr.recipe_list_id = rl.id)
    )
    FROM cooktime.recipe_lists rl
    WHERE rl.owner_id = p_user_id
      AND (p_filter IS NULL OR rl.name = p_filter OR rl.id::text = p_filter)
    ORDER BY rl.creation_date DESC;
END;
$$ LANGUAGE plpgsql;

-- Update get_recipe_list_with_recipes to return slug
DROP FUNCTION IF EXISTS cooktime.get_recipe_list_with_recipes(uuid);
CREATE OR REPLACE FUNCTION cooktime.get_recipe_list_with_recipes(p_list_id uuid)
RETURNS jsonb AS $$
BEGIN
    RETURN (
        SELECT jsonb_build_object(
            'id', rl.id,
            'name', rl.name,
            'slug', rl.slug,
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
        WHERE rl.id = p_list_id
    );
END;
$$ LANGUAGE plpgsql;

-- Function to get list by slug (for URL lookups)
DROP FUNCTION IF EXISTS cooktime.get_recipe_list_by_slug(text);
CREATE OR REPLACE FUNCTION cooktime.get_recipe_list_by_slug(p_slug text)
RETURNS jsonb AS $$
DECLARE
    list_id uuid;
BEGIN
    SELECT id INTO list_id
    FROM cooktime.recipe_lists
    WHERE slug = p_slug;
    
    IF list_id IS NULL THEN
        RETURN NULL;
    END IF;
    
    RETURN cooktime.get_recipe_list_with_recipes(list_id);
END;
$$ LANGUAGE plpgsql;
