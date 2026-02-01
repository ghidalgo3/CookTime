-- PostgreSQL Migration: Image Ordering Support
-- Adds position column to images table and updates related functions

-- Add position column to images table
ALTER TABLE cooktime.images 
ADD COLUMN IF NOT EXISTS position integer DEFAULT 0;

-- Create index for efficient ordering queries
CREATE INDEX IF NOT EXISTS idx_images_position ON cooktime.images(recipe_id, position);

-- Update get_recipe_images to order by position (then uploaded_date as fallback)
CREATE OR REPLACE FUNCTION cooktime.get_recipe_images(p_recipe_id uuid)
RETURNS jsonb AS $$
BEGIN
    RETURN COALESCE(
        (SELECT jsonb_agg(
            jsonb_build_object(
                'id', i.id,
                'url', i.storage_url
            ) ORDER BY i.position ASC, i.uploaded_date DESC
        )
        FROM cooktime.images i
        WHERE i.recipe_id = p_recipe_id),
        '[]'::jsonb
    );
END;
$$ LANGUAGE plpgsql;

-- Function to delete a single image by ID
CREATE OR REPLACE FUNCTION cooktime.delete_image(p_image_id uuid)
RETURNS void AS $$
BEGIN
    DELETE FROM cooktime.images WHERE id = p_image_id;
END;
$$ LANGUAGE plpgsql;

-- Function to get image info (for blob deletion)
CREATE OR REPLACE FUNCTION cooktime.get_image_info(p_image_id uuid)
RETURNS jsonb AS $$
BEGIN
    RETURN (
        SELECT jsonb_build_object(
            'id', i.id,
            'url', i.storage_url,
            'recipeId', i.recipe_id
        )
        FROM cooktime.images i
        WHERE i.id = p_image_id
    );
END;
$$ LANGUAGE plpgsql;

-- Function to reorder images for a recipe
CREATE OR REPLACE FUNCTION cooktime.reorder_recipe_images(p_recipe_id uuid, p_image_ids uuid[])
RETURNS void AS $$
DECLARE
    v_position integer := 0;
    v_image_id uuid;
BEGIN
    FOREACH v_image_id IN ARRAY p_image_ids
    LOOP
        UPDATE cooktime.images 
        SET position = v_position 
        WHERE id = v_image_id AND recipe_id = p_recipe_id;
        v_position := v_position + 1;
    END LOOP;
END;
$$ LANGUAGE plpgsql;

-- Function to get image count for a recipe
CREATE OR REPLACE FUNCTION cooktime.get_recipe_image_count(p_recipe_id uuid)
RETURNS integer AS $$
BEGIN
    RETURN (SELECT COUNT(*)::integer FROM cooktime.images WHERE recipe_id = p_recipe_id);
END;
$$ LANGUAGE plpgsql;

-- Update create image to set position to max + 1
CREATE OR REPLACE FUNCTION cooktime.create_image(p_image_id uuid, p_storage_url text, p_recipe_id uuid)
RETURNS void AS $$
DECLARE
    v_max_position integer;
BEGIN
    SELECT COALESCE(MAX(position), -1) + 1 INTO v_max_position
    FROM cooktime.images
    WHERE recipe_id = p_recipe_id;
    
    INSERT INTO cooktime.images (id, storage_url, recipe_id, position)
    VALUES (p_image_id, p_storage_url, p_recipe_id, v_max_position);
END;
$$ LANGUAGE plpgsql;