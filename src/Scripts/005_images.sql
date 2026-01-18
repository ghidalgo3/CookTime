-- PostgreSQL Migration: Images Schema Changes
-- Remove binary data column, add storage URL

-- Note: This migration assumes images have already been uploaded to Azure Blob Storage
-- Run the image upload process before applying this migration

-- Add storage_url column if it doesn't exist
ALTER TABLE cooktime.images 
ADD COLUMN IF NOT EXISTS storage_url text DEFAULT '';

-- Drop the binary data column (after images are uploaded)
-- Uncomment when ready to apply:
-- ALTER TABLE cooktime.images DROP COLUMN IF EXISTS image_data CASCADE;

-- Make storage_url required after migration
-- Uncomment when ready to apply:
-- ALTER TABLE cooktime.images ALTER COLUMN storage_url SET NOT NULL;
-- ALTER TABLE cooktime.images ALTER COLUMN storage_url DROP DEFAULT;

-- Create image record
CREATE OR REPLACE FUNCTION cooktime.create_image(image_json jsonb)
RETURNS uuid AS $$
DECLARE
    image_id uuid;
BEGIN
    INSERT INTO cooktime.images (
        storage_url,
        static_image_name,
        recipe_id,
        ingredient_id
    ) VALUES (
        image_json->>'storageUrl',
        image_json->>'staticImageName',
        (image_json->>'recipeId')::uuid,
        (image_json->>'ingredientId')::uuid
    )
    RETURNING id INTO image_id;
    
    RETURN image_id;
END;
$$ LANGUAGE plpgsql;

-- Get ingredient images
CREATE OR REPLACE FUNCTION cooktime.get_ingredient_images(ingredient_id uuid)
RETURNS SETOF jsonb AS $$
BEGIN
    RETURN QUERY
    SELECT jsonb_build_object(
        'id', i.id,
        'storageUrl', i.storage_url,
        'uploadedDate', i.uploaded_date,
        'staticImageName', i.static_image_name
    )
    FROM cooktime.images i
    WHERE i.ingredient_id = get_ingredient_images.ingredient_id
    ORDER BY i.uploaded_date DESC;
END;
$$ LANGUAGE plpgsql;
