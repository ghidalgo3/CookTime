-- PostgreSQL Migration: User Custom Lists Enhancements
-- Adds delete and update operations for recipe lists

-- Add unique constraint on recipe_requirements to prevent duplicate recipes in a list
-- This allows us to use ON CONFLICT to increment quantity instead of creating duplicates
CREATE UNIQUE INDEX IF NOT EXISTS idx_recipe_requirements_list_recipe_unique 
ON cooktime.recipe_requirements(recipe_list_id, recipe_id);

-- Update add_recipe_to_list to increment quantity if recipe already exists in list
DROP FUNCTION IF EXISTS cooktime.add_recipe_to_list(uuid, uuid, double precision);
CREATE OR REPLACE FUNCTION cooktime.add_recipe_to_list(
    p_list_id uuid,
    p_recipe_id uuid,
    p_quantity double precision DEFAULT 1.0
)
RETURNS uuid AS $$
DECLARE
    requirement_id uuid;
BEGIN
    -- Validate list exists
    IF NOT EXISTS (SELECT 1 FROM cooktime.recipe_lists WHERE id = p_list_id) THEN
        RAISE EXCEPTION 'Recipe list with id % does not exist', p_list_id;
    END IF;
    
    -- Validate recipe exists
    IF NOT EXISTS (SELECT 1 FROM cooktime.recipes WHERE id = p_recipe_id) THEN
        RAISE EXCEPTION 'Recipe with id % does not exist', p_recipe_id;
    END IF;
    
    -- Insert or update quantity if recipe already exists in list
    INSERT INTO cooktime.recipe_requirements (
        recipe_list_id,
        recipe_id,
        quantity
    ) VALUES (
        p_list_id,
        p_recipe_id,
        p_quantity
    )
    ON CONFLICT (recipe_list_id, recipe_id) 
    DO UPDATE SET quantity = cooktime.recipe_requirements.quantity + EXCLUDED.quantity
    RETURNING id INTO requirement_id;
    
    RETURN requirement_id;
END;
$$ LANGUAGE plpgsql;

-- Delete a recipe list by ID (verifies ownership)
DROP FUNCTION IF EXISTS cooktime.delete_recipe_list(uuid, uuid);
CREATE OR REPLACE FUNCTION cooktime.delete_recipe_list(
    p_user_id uuid,
    p_list_id uuid
)
RETURNS boolean AS $$
DECLARE
    list_owner uuid;
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
    
    -- Delete the list (cascade will handle recipe_requirements and selected_ingredients)
    DELETE FROM cooktime.recipe_lists WHERE id = p_list_id;
    
    RETURN true;
END;
$$ LANGUAGE plpgsql;

-- Update recipe list metadata (name, description, isPublic)
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
    
    -- Update only the fields that are not null
    UPDATE cooktime.recipe_lists
    SET 
        name = COALESCE(p_name, name),
        description = COALESCE(p_description, description),
        is_public = COALESCE(p_is_public, is_public)
    WHERE id = p_list_id;
    
    -- Return the updated list
    SELECT jsonb_build_object(
        'id', rl.id,
        'name', rl.name,
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