-- PostgreSQL Migration: Write Stored Procedures
-- Procedures for creating, updating, and deleting data

-- Create recipe from JSON
CREATE OR REPLACE FUNCTION cooktime.create_recipe(recipe_json jsonb)
RETURNS uuid AS $$
DECLARE
    new_recipe_id uuid;
    component_item jsonb;
    component_id uuid;
    ingredient_item jsonb;
    category_item jsonb;
    v_ingredient_id uuid;
BEGIN
    -- Validate input
    PERFORM cooktime.validate_recipe_json(recipe_json);
    
    -- Insert recipe
    INSERT INTO cooktime.recipes (
        name,
        description,
        cooking_minutes,
        servings,
        prep_minutes,
        calories,
        source,
        owner_id
    ) VALUES (
        recipe_json->>'name',
        recipe_json->>'description',
        (recipe_json->>'cookingMinutes')::double precision,
        (recipe_json->>'servings')::integer,
        (recipe_json->>'prepMinutes')::double precision,
        (recipe_json->>'calories')::integer,
        recipe_json->>'source',
        (recipe_json->>'ownerId')::uuid
    )
    RETURNING id INTO new_recipe_id;
    
    -- Insert components, steps, and ingredients
    FOR component_item IN SELECT * FROM jsonb_array_elements(recipe_json->'components')
    LOOP
        PERFORM cooktime.validate_component_json(component_item);
        
        -- Insert component with steps array
        INSERT INTO cooktime.recipe_components (
            name,
            position,
            steps,
            recipe_id
        ) VALUES (
            component_item->>'name',
            (component_item->>'position')::integer,
            ARRAY(SELECT jsonb_array_elements_text(COALESCE(component_item->'steps', '[]'::jsonb))),
            new_recipe_id
        )
        RETURNING id INTO component_id;
        
        -- Insert ingredient requirements for this component
        FOR ingredient_item IN SELECT * FROM jsonb_array_elements(component_item->'ingredients')
        LOOP
            PERFORM cooktime.validate_ingredient_requirement_json(ingredient_item);
            
            -- Ensure ingredient exists (creates if isNew=true)
            v_ingredient_id := cooktime.ensure_ingredient_exists(ingredient_item->'ingredient');
            
            INSERT INTO cooktime.ingredient_requirements (
                ingredient_id,
                recipe_component_id,
                unit,
                quantity,
                position,
                description
            ) VALUES (
                v_ingredient_id,
                component_id,
                (ingredient_item->>'unit')::cooktime.unit,
                (ingredient_item->>'quantity')::double precision,
                (ingredient_item->>'position')::integer,
                ingredient_item->>'text'
            );
        END LOOP;
    END LOOP;
    
    -- Insert categories if provided
    IF recipe_json ? 'categoryIds' THEN
        FOR category_item IN SELECT * FROM jsonb_array_elements(recipe_json->'categoryIds')
        LOOP
            INSERT INTO cooktime.category_recipe (category_id, recipe_id)
            VALUES ((category_item#>>'{}')::uuid, new_recipe_id)
            ON CONFLICT DO NOTHING;
        END LOOP;
    END IF;
    
    RETURN new_recipe_id;
END;
$$ LANGUAGE plpgsql;

-- Update recipe from JSON
CREATE OR REPLACE FUNCTION cooktime.update_recipe(p_recipe_id uuid, recipe_json jsonb)
RETURNS void AS $$
DECLARE
    component_item jsonb;
    component_id uuid;
    ingredient_item jsonb;
    category_item jsonb;
    v_ingredient_id uuid;
BEGIN
    -- Validate input
    PERFORM cooktime.validate_recipe_json(recipe_json);
    
    -- Check recipe exists
    IF NOT EXISTS (SELECT 1 FROM cooktime.recipes WHERE id = p_recipe_id) THEN
        RAISE EXCEPTION 'Recipe with id % does not exist', p_recipe_id;
    END IF;
    
    -- Update recipe fields
    UPDATE cooktime.recipes SET
        name = recipe_json->>'name',
        description = recipe_json->>'description',
        cooking_minutes = (recipe_json->>'cookingMinutes')::double precision,
        servings = (recipe_json->>'servings')::integer,
        prep_minutes = (recipe_json->>'prepMinutes')::double precision,
        calories = (recipe_json->>'calories')::integer,
        source = recipe_json->>'source'
    WHERE id = p_recipe_id;
    
    -- Delete existing components (will cascade to ingredient requirements)
    DELETE FROM cooktime.recipe_components WHERE recipe_id = p_recipe_id;
    
    -- Insert new components and ingredients
    FOR component_item IN SELECT * FROM jsonb_array_elements(recipe_json->'components')
    LOOP
        PERFORM cooktime.validate_component_json(component_item);
        
        INSERT INTO cooktime.recipe_components (
            name,
            position,
            steps,
            recipe_id
        ) VALUES (
            component_item->>'name',
            (component_item->>'position')::integer,
            ARRAY(SELECT jsonb_array_elements_text(COALESCE(component_item->'steps', '[]'::jsonb))),
            p_recipe_id
        )
        RETURNING id INTO component_id;
        
        FOR ingredient_item IN SELECT * FROM jsonb_array_elements(component_item->'ingredients')
        LOOP
            PERFORM cooktime.validate_ingredient_requirement_json(ingredient_item);
            
            -- Ensure ingredient exists (creates if isNew=true)
            v_ingredient_id := cooktime.ensure_ingredient_exists(ingredient_item->'ingredient');
            
            INSERT INTO cooktime.ingredient_requirements (
                ingredient_id,
                recipe_component_id,
                unit,
                quantity,
                position,
                description
            ) VALUES (
                v_ingredient_id,
                component_id,
                (ingredient_item->>'unit')::cooktime.unit,
                (ingredient_item->>'quantity')::double precision,
                (ingredient_item->>'position')::integer,
                ingredient_item->>'text'
            );
        END LOOP;
    END LOOP;
    
    -- Update categories if provided
    IF recipe_json ? 'categoryIds' THEN
        DELETE FROM cooktime.category_recipe WHERE recipe_id = p_recipe_id;
        
        FOR category_item IN SELECT * FROM jsonb_array_elements(recipe_json->'categoryIds')
        LOOP
            INSERT INTO cooktime.category_recipe (category_id, recipe_id)
            VALUES ((category_item#>>'{}')::uuid, p_recipe_id)
            ON CONFLICT DO NOTHING;
        END LOOP;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Delete recipe
CREATE OR REPLACE FUNCTION cooktime.delete_recipe(p_recipe_id uuid)
RETURNS void AS $$
BEGIN
    -- Check recipe exists
    IF NOT EXISTS (SELECT 1 FROM cooktime.recipes WHERE id = p_recipe_id) THEN
        RAISE EXCEPTION 'Recipe with id % does not exist', p_recipe_id;
    END IF;
    
    -- Delete recipe (cascades to components, steps, requirements)
    DELETE FROM cooktime.recipes WHERE id = p_recipe_id;
END;
$$ LANGUAGE plpgsql;

-- Create or update nutrition data
CREATE OR REPLACE FUNCTION cooktime.create_or_update_nutrition_data(nutrition_json jsonb)
RETURNS uuid AS $$
DECLARE
    nutrition_id uuid;
    existing_id uuid;
BEGIN
    -- Check if nutrition data with same source_ids exists
    SELECT id INTO existing_id
    FROM cooktime.nutrition_facts
    WHERE source_ids = nutrition_json->'sourceIds';
    
    IF existing_id IS NOT NULL THEN
        -- Update existing
        UPDATE cooktime.nutrition_facts SET
            names = ARRAY(SELECT jsonb_array_elements_text(nutrition_json->'names')),
            unit_mass = (nutrition_json->>'unitMass')::double precision,
            density = (nutrition_json->>'density')::double precision,
            nutrition_data = nutrition_json->'nutritionData',
            count_regex = nutrition_json->>'countRegex',
            dataset = nutrition_json->>'dataset'
        WHERE id = existing_id;
        
        RETURN existing_id;
    ELSE
        -- Insert new
        INSERT INTO cooktime.nutrition_facts (
            source_ids,
            names,
            unit_mass,
            density,
            nutrition_data,
            count_regex,
            dataset
        ) VALUES (
            nutrition_json->'sourceIds',
            ARRAY(SELECT jsonb_array_elements_text(nutrition_json->'names')),
            (nutrition_json->>'unitMass')::double precision,
            (nutrition_json->>'density')::double precision,
            nutrition_json->'nutritionData',
            nutrition_json->>'countRegex',
            nutrition_json->>'dataset'
        )
        RETURNING id INTO nutrition_id;
        
        RETURN nutrition_id;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Import nutrition facts from USDA data (SR Legacy or Branded)
CREATE OR REPLACE FUNCTION cooktime.import_nutrition_facts(nutrition_json jsonb, p_dataset text)
RETURNS uuid AS $$
DECLARE
    nutrition_id uuid;
    existing_id uuid;
    v_source_ids jsonb;
BEGIN
    -- Build source_ids based on dataset
    IF p_dataset = 'usda_sr_legacy' THEN
        v_source_ids = jsonb_build_object(
            'ndbNumber', nutrition_json->>'ndbNumber',
            'fdcId', nutrition_json->>'fdcId'
        );
    ELSIF p_dataset = 'usda_branded' THEN
        v_source_ids = jsonb_build_object(
            'gtinUpc', nutrition_json->>'gtinUpc',
            'fdcId', nutrition_json->>'fdcId'
        );
    ELSE
        RAISE EXCEPTION 'Unknown dataset: %', p_dataset;
    END IF;
    
    -- Check if nutrition data with same source_ids exists
    SELECT id INTO existing_id
    FROM cooktime.nutrition_facts
    WHERE source_ids = v_source_ids;
    
    IF existing_id IS NOT NULL THEN
        RETURN existing_id;  -- Already exists, skip
    END IF;
    
    -- Insert new
    INSERT INTO cooktime.nutrition_facts (
        source_ids,
        names,
        unit_mass,
        density,
        nutrition_data,
        count_regex,
        dataset
    ) VALUES (
        v_source_ids,
        ARRAY[nutrition_json->>'description'],
        NULL,  -- Will be calculated later if needed
        NULL,  -- Will be calculated later if needed
        nutrition_json,  -- Store the whole USDA record as nutrition_data
        NULL,
        p_dataset
    )
    RETURNING id INTO nutrition_id;
    
    RETURN nutrition_id;
END;
$$ LANGUAGE plpgsql;

-- Update the density value for a nutrition_facts record
CREATE OR REPLACE FUNCTION cooktime.update_nutrition_facts_density(
    p_nutrition_facts_id uuid,
    p_density double precision
)
RETURNS void AS $$
BEGIN
    UPDATE cooktime.nutrition_facts
    SET density = p_density
    WHERE id = p_nutrition_facts_id;
END;
$$ LANGUAGE plpgsql;

-- Associate ingredient with USDA SR Legacy nutrition facts by NDB number
CREATE OR REPLACE FUNCTION cooktime.associate_ingredient_sr_legacy(
    p_ingredient_id uuid,
    p_ndb_number integer
)
RETURNS boolean AS $$
DECLARE
    v_nutrition_facts_id uuid;
BEGIN
    -- Find the nutrition_facts record with matching ndbNumber
    SELECT id INTO v_nutrition_facts_id
    FROM cooktime.nutrition_facts
    WHERE dataset = 'usda_sr_legacy'
      AND (source_ids->>'ndbNumber')::integer = p_ndb_number;
    
    -- If not found, return false
    IF v_nutrition_facts_id IS NULL THEN
        RETURN false;
    END IF;
    
    -- Update the ingredient with the nutrition_facts_id
    UPDATE cooktime.ingredients
    SET nutrition_facts_id = v_nutrition_facts_id
    WHERE id = p_ingredient_id;
    
    RETURN true;
END;
$$ LANGUAGE plpgsql;

-- Associate ingredient with USDA Branded nutrition facts by GTIN/UPC
CREATE OR REPLACE FUNCTION cooktime.associate_ingredient_branded(
    p_ingredient_id uuid,
    p_gtin_upc text
)
RETURNS boolean AS $$
DECLARE
    v_nutrition_facts_id uuid;
BEGIN
    -- Find the nutrition_facts record with matching gtinUpc
    SELECT id INTO v_nutrition_facts_id
    FROM cooktime.nutrition_facts
    WHERE dataset = 'usda_branded'
      AND source_ids->>'gtinUpc' = p_gtin_upc;
    
    -- If not found, return false
    IF v_nutrition_facts_id IS NULL THEN
        RETURN false;
    END IF;
    
    -- Update the ingredient with the nutrition_facts_id
    UPDATE cooktime.ingredients
    SET nutrition_facts_id = v_nutrition_facts_id
    WHERE id = p_ingredient_id;
    
    RETURN true;
END;
$$ LANGUAGE plpgsql;

-- Update ingredient from admin internal update view
-- This function handles updating ingredient name, expected unit mass, and nutrition association
CREATE OR REPLACE FUNCTION cooktime.update_ingredient_internal(
    p_ingredient_id uuid,
    p_ingredient_names text,
    p_expected_unit_mass double precision,
    p_ndb_number integer,
    p_gtin_upc text,
    p_count_regex text
)
RETURNS jsonb AS $$
DECLARE
    v_nutrition_facts_id uuid;
    v_nutrition_description text;
    v_actual_ndb integer;
    v_actual_gtin text;
BEGIN
    -- Update ingredient name and expected unit mass
    UPDATE cooktime.ingredients
    SET name = p_ingredient_names,
        expected_unit_mass_kg = p_expected_unit_mass
    WHERE id = p_ingredient_id;
    
    -- Try to associate with nutrition facts by NDB number first
    IF p_ndb_number IS NOT NULL AND p_ndb_number > 0 THEN
        SELECT id INTO v_nutrition_facts_id
        FROM cooktime.nutrition_facts
        WHERE dataset = 'usda_sr_legacy'
          AND (source_ids->>'ndbNumber')::integer = p_ndb_number;
        
        IF v_nutrition_facts_id IS NOT NULL THEN
            UPDATE cooktime.ingredients
            SET nutrition_facts_id = v_nutrition_facts_id
            WHERE id = p_ingredient_id;
            
            -- Update count_regex on nutrition_facts if provided
            IF p_count_regex IS NOT NULL AND p_count_regex <> '' THEN
                UPDATE cooktime.nutrition_facts
                SET count_regex = p_count_regex
                WHERE id = v_nutrition_facts_id;
            END IF;
        END IF;
    -- Try GTIN/UPC if NDB not provided or not found
    ELSIF p_gtin_upc IS NOT NULL AND p_gtin_upc <> '' THEN
        SELECT id INTO v_nutrition_facts_id
        FROM cooktime.nutrition_facts
        WHERE dataset = 'usda_branded'
          AND source_ids->>'gtinUpc' = p_gtin_upc;
        
        IF v_nutrition_facts_id IS NOT NULL THEN
            UPDATE cooktime.ingredients
            SET nutrition_facts_id = v_nutrition_facts_id
            WHERE id = p_ingredient_id;
            
            -- Update count_regex on nutrition_facts if provided
            IF p_count_regex IS NOT NULL AND p_count_regex <> '' THEN
                UPDATE cooktime.nutrition_facts
                SET count_regex = p_count_regex
                WHERE id = v_nutrition_facts_id;
            END IF;
        END IF;
    END IF;
    
    -- Get the current state for the response
    SELECT 
        i.nutrition_facts_id,
        nf.nutrition_data->>'description',
        COALESCE((nf.source_ids->>'ndbNumber')::integer, 0),
        COALESCE(nf.source_ids->>'gtinUpc', '')
    INTO v_nutrition_facts_id, v_nutrition_description, v_actual_ndb, v_actual_gtin
    FROM cooktime.ingredients i
    LEFT JOIN cooktime.nutrition_facts nf ON nf.id = i.nutrition_facts_id
    WHERE i.id = p_ingredient_id;
    
    RETURN jsonb_build_object(
        'ingredientId', p_ingredient_id,
        'ingredientNames', p_ingredient_names,
        'expectedUnitMass', p_expected_unit_mass::text,
        'ndbNumber', v_actual_ndb,
        'gtinUpc', v_actual_gtin,
        'countRegex', COALESCE(p_count_regex, ''),
        'nutritionDescription', COALESCE(v_nutrition_description, '')
    );
END;
$$ LANGUAGE plpgsql;

-- Create ingredient
CREATE OR REPLACE FUNCTION cooktime.create_ingredient(ingredient_json jsonb)
RETURNS uuid AS $$
DECLARE
    ingredient_id uuid;
BEGIN
    PERFORM cooktime.validate_ingredient_json(ingredient_json);
    
    INSERT INTO cooktime.ingredients (
        name,
        default_serving_size_unit,
        nutrition_facts_id
    ) VALUES (
        ingredient_json->>'name',
        COALESCE((ingredient_json->>'defaultServingSizeUnit')::double precision, 0.1),
        (ingredient_json->>'nutritionFactsId')::uuid
    )
    RETURNING id INTO ingredient_id;
    
    RETURN ingredient_id;
END;
$$ LANGUAGE plpgsql;

-- Create recipe list
CREATE OR REPLACE FUNCTION cooktime.create_recipe_list(list_json jsonb)
RETURNS uuid AS $$
DECLARE
    list_id uuid;
BEGIN
    PERFORM cooktime.validate_recipe_list_json(list_json);
    
    INSERT INTO cooktime.recipe_lists (
        name,
        description,
        is_public,
        owner_id
    ) VALUES (
        list_json->>'name',
        list_json->>'description',
        COALESCE((list_json->>'isPublic')::boolean, false),
        (list_json->>'ownerId')::uuid
    )
    RETURNING id INTO list_id;
    
    RETURN list_id;
END;
$$ LANGUAGE plpgsql;

-- Add recipe to list
CREATE OR REPLACE FUNCTION cooktime.add_recipe_to_list(
    list_id uuid,
    recipe_id uuid,
    quantity_multiplier double precision DEFAULT 1.0
)
RETURNS uuid AS $$
DECLARE
    requirement_id uuid;
BEGIN
    -- Validate list exists
    IF NOT EXISTS (SELECT 1 FROM cooktime.recipe_lists WHERE id = list_id) THEN
        RAISE EXCEPTION 'Recipe list with id % does not exist', list_id;
    END IF;
    
    -- Validate recipe exists
    IF NOT EXISTS (SELECT 1 FROM cooktime.recipes WHERE id = recipe_id) THEN
        RAISE EXCEPTION 'Recipe with id % does not exist', recipe_id;
    END IF;
    
    INSERT INTO cooktime.recipe_requirements (
        recipe_list_id,
        recipe_id,
        quantity
    ) VALUES (
        list_id,
        recipe_id,
        quantity_multiplier
    )
    RETURNING id INTO requirement_id;
    
    RETURN requirement_id;
END;
$$ LANGUAGE plpgsql;

-- Create category
CREATE OR REPLACE FUNCTION cooktime.create_category(category_name text)
RETURNS uuid AS $$
DECLARE
    category_id uuid;
BEGIN
    INSERT INTO cooktime.categories (name)
    VALUES (category_name)
    ON CONFLICT (name) DO UPDATE SET name = EXCLUDED.name
    RETURNING id INTO category_id;
    
    RETURN category_id;
END;
$$ LANGUAGE plpgsql;

-- =============================================================================
-- Import Functions (preserve existing IDs for data migration)
-- Note: NDJSON files use PascalCase keys (Id, Name, etc.)
-- =============================================================================

-- Import ingredient with existing ID
CREATE OR REPLACE FUNCTION cooktime.import_ingredient(ingredient_json jsonb)
RETURNS uuid AS $$
DECLARE
    ingredient_id uuid;
BEGIN
    INSERT INTO cooktime.ingredients (
        id,
        name,
        expected_unit_mass_kg
    ) VALUES (
        (ingredient_json->>'Id')::uuid,
        ingredient_json->>'Name',
        COALESCE((ingredient_json->>'ExpectedUnitMass')::double precision, 0.1)
    )
    ON CONFLICT (id) DO NOTHING
    RETURNING id INTO ingredient_id;
    
    RETURN ingredient_id;
END;
$$ LANGUAGE plpgsql;

-- Import category with existing ID
CREATE OR REPLACE FUNCTION cooktime.import_category(category_json jsonb)
RETURNS uuid AS $$
DECLARE
    cat_id uuid;
BEGIN
    INSERT INTO cooktime.categories (id, name)
    VALUES (
        (category_json->>'Id')::uuid,
        category_json->>'Name'
    )
    ON CONFLICT (id) DO NOTHING
    RETURNING id INTO cat_id;
    
    RETURN cat_id;
END;
$$ LANGUAGE plpgsql;

-- Import recipe with existing IDs (recipe, components, ingredient requirements)
CREATE OR REPLACE FUNCTION cooktime.import_recipe(recipe_json jsonb)
RETURNS uuid AS $$
DECLARE
    recipe_id uuid;
    component_item jsonb;
    component_id uuid;
    ingredient_item jsonb;
    category_id uuid;
BEGIN
    -- Insert recipe with provided ID
    INSERT INTO cooktime.recipes (
        id,
        name,
        cooking_minutes,
        servings,
        source,
        owner_id
    ) VALUES (
        (recipe_json->>'Id')::uuid,
        recipe_json->>'Name',
        (recipe_json->>'CooktimeMinutes')::double precision,
        (recipe_json->>'ServingsProduced')::integer,
        recipe_json->>'Source',
        (recipe_json->>'OwnerId')::uuid
    )
    ON CONFLICT (id) DO NOTHING
    RETURNING id INTO recipe_id;
    
    -- If recipe already exists, return null
    IF recipe_id IS NULL THEN
        RETURN NULL;
    END IF;
    
    -- Link categories
    FOR category_id IN SELECT (cat->>'Id')::uuid FROM jsonb_array_elements(recipe_json->'Categories') cat
    LOOP
        INSERT INTO cooktime.category_recipe (category_id, recipe_id)
        VALUES (category_id, recipe_id)
        ON CONFLICT DO NOTHING;
    END LOOP;
    
    -- Insert components
    FOR component_item IN SELECT * FROM jsonb_array_elements(recipe_json->'RecipeComponents')
    LOOP
        INSERT INTO cooktime.recipe_components (
            id,
            name,
            position,
            steps,
            recipe_id
        ) VALUES (
            (component_item->>'Id')::uuid,
            component_item->>'Name',
            (component_item->>'Position')::integer,
            ARRAY(SELECT jsonb_array_elements_text(
                COALESCE(
                    (SELECT jsonb_agg(s->>'Text') FROM jsonb_array_elements(component_item->'Steps') s),
                    '[]'::jsonb
                )
            )),
            recipe_id
        )
        ON CONFLICT (id) DO NOTHING
        RETURNING id INTO component_id;
        
        -- Skip ingredient requirements if component already exists
        IF component_id IS NULL THEN
            CONTINUE;
        END IF;
        
        -- Insert ingredient requirements (only if ingredient exists)
        FOR ingredient_item IN SELECT * FROM jsonb_array_elements(component_item->'Ingredients')
        LOOP
            -- Skip if ingredient doesn't exist
            IF NOT EXISTS (
                SELECT 1 FROM cooktime.ingredients 
                WHERE id = (ingredient_item->>'Id')::uuid
            ) THEN
                CONTINUE;
            END IF;

            INSERT INTO cooktime.ingredient_requirements (
                ingredient_id,
                recipe_component_id,
                unit,
                quantity,
                position,
                description
            ) VALUES (
                (ingredient_item->>'Id')::uuid,
                component_id,
                cooktime.map_unit_code((ingredient_item->>'Unit')::integer),
                (ingredient_item->>'Quantity')::double precision,
                (ingredient_item->>'Position')::integer,
                ingredient_item->>'Text'
            )
            ON CONFLICT DO NOTHING;
        END LOOP;
    END LOOP;
    
    RETURN recipe_id;
END;
$$ LANGUAGE plpgsql;

-- Import image with existing ID
CREATE OR REPLACE FUNCTION cooktime.import_image(image_json jsonb)
RETURNS uuid AS $$
DECLARE
    image_id uuid;
BEGIN
    INSERT INTO cooktime.images (
        id,
        storage_url,
        uploaded_date,
        static_image_name,
        recipe_id
    ) VALUES (
        (image_json->>'Id')::uuid,
        image_json->>'StorageUrl',
        COALESCE((image_json->>'UploadedDate')::timestamptz, now()),
        image_json->>'Name',
        (image_json->>'RecipeId')::uuid
    )
    ON CONFLICT (id) DO NOTHING
    RETURNING id INTO image_id;
    
    RETURN image_id;
END;
$$ LANGUAGE plpgsql;

-- Helper function to map integer unit codes to enum values
CREATE OR REPLACE FUNCTION cooktime.map_unit_code(unit_code integer)
RETURNS cooktime.unit AS $$
BEGIN
    RETURN CASE unit_code
        -- Volumetric
        WHEN 100 THEN 'tablespoon'::cooktime.unit
        WHEN 101 THEN 'teaspoon'::cooktime.unit
        WHEN 102 THEN 'milliliter'::cooktime.unit
        WHEN 103 THEN 'cup'::cooktime.unit
        WHEN 104 THEN 'fluid_ounce'::cooktime.unit
        WHEN 105 THEN 'pint'::cooktime.unit
        WHEN 106 THEN 'quart'::cooktime.unit
        WHEN 107 THEN 'gallon'::cooktime.unit
        WHEN 108 THEN 'liter'::cooktime.unit
        -- Count
        WHEN 1000 THEN 'count'::cooktime.unit
        -- Mass
        WHEN 2000 THEN 'ounce'::cooktime.unit
        WHEN 2001 THEN 'pound'::cooktime.unit
        WHEN 2002 THEN 'milligram'::cooktime.unit
        WHEN 2003 THEN 'gram'::cooktime.unit
        WHEN 2004 THEN 'kilogram'::cooktime.unit
        ELSE NULL
    END;
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- Helper function to map string unit names to enum values
CREATE OR REPLACE FUNCTION cooktime.map_unit_name(unit_name text)
RETURNS cooktime.unit AS $$
BEGIN
    RETURN CASE lower(unit_name)
        WHEN 'tablespoon' THEN 'tablespoon'::cooktime.unit
        WHEN 'teaspoon' THEN 'teaspoon'::cooktime.unit
        WHEN 'milliliter' THEN 'milliliter'::cooktime.unit
        WHEN 'cup' THEN 'cup'::cooktime.unit
        WHEN 'fluid_ounce' THEN 'fluid_ounce'::cooktime.unit
        WHEN 'pint' THEN 'pint'::cooktime.unit
        WHEN 'quart' THEN 'quart'::cooktime.unit
        WHEN 'gallon' THEN 'gallon'::cooktime.unit
        WHEN 'liter' THEN 'liter'::cooktime.unit
        WHEN 'count' THEN 'count'::cooktime.unit
        WHEN 'ounce' THEN 'ounce'::cooktime.unit
        WHEN 'pound' THEN 'pound'::cooktime.unit
        WHEN 'milligram' THEN 'milligram'::cooktime.unit
        WHEN 'gram' THEN 'gram'::cooktime.unit
        WHEN 'kilogram' THEN 'kilogram'::cooktime.unit
        ELSE NULL
    END;
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- Import ingredient requirement with existing IDs
CREATE OR REPLACE FUNCTION cooktime.import_ingredient_requirement(req_json jsonb)
RETURNS uuid AS $$
DECLARE
    req_id uuid;
    component_id uuid;
    ingredient_id uuid;
BEGIN
    component_id := (req_json->>'RecipeComponentId')::uuid;
    ingredient_id := (req_json->>'IngredientId')::uuid;
    
    -- Skip if component or ingredient doesn't exist
    IF NOT EXISTS (SELECT 1 FROM cooktime.recipe_components WHERE id = component_id) THEN
        RETURN NULL;
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM cooktime.ingredients WHERE id = ingredient_id) THEN
        RETURN NULL;
    END IF;
    
    INSERT INTO cooktime.ingredient_requirements (
        id,
        recipe_component_id,
        ingredient_id,
        unit,
        quantity,
        position,
        description
    ) VALUES (
        (req_json->>'Id')::uuid,
        component_id,
        ingredient_id,
        cooktime.map_unit_name(req_json->>'Unit'),
        (req_json->>'Quantity')::double precision,
        COALESCE((req_json->>'Position')::integer, 0),
        req_json->>'Text'
    )
    ON CONFLICT (id) DO NOTHING
    RETURNING id INTO req_id;
    
    RETURN req_id;
END;
$$ LANGUAGE plpgsql;

-- Import user from AspNetUsers table
CREATE OR REPLACE FUNCTION cooktime.import_user(user_json jsonb)
RETURNS uuid AS $$
DECLARE
    user_id uuid;
BEGIN
    INSERT INTO cooktime.users (
        id,
        provider,
        provider_user_id,
        email,
        display_name,
        roles
    ) VALUES (
        (user_json->>'Id')::uuid,
        'cooktime',
        user_json->>'Id',
        user_json->>'Email',
        COALESCE(user_json->>'UserName', user_json->>'Email', 'User'),
        ARRAY['User']
    )
    ON CONFLICT (id) DO NOTHING
    RETURNING id INTO user_id;
    
    RETURN user_id;
END;
$$ LANGUAGE plpgsql;

-- Import review from Reviews table
CREATE OR REPLACE FUNCTION cooktime.import_review(review_json jsonb)
RETURNS uuid AS $$
DECLARE
    review_id uuid;
BEGIN
    INSERT INTO cooktime.reviews (
        id,
        created_date,
        last_modified_date,
        owner_id,
        recipe_id,
        rating,
        comment
    ) VALUES (
        (review_json->>'Id')::uuid,
        (review_json->>'CreatedAt')::timestamptz,
        (review_json->>'LastModified')::timestamptz,
        (review_json->>'OwnerId')::uuid,
        (review_json->>'RecipeId')::uuid,
        (review_json->>'Rating')::integer,
        review_json->>'Text'
    )
    ON CONFLICT (id) DO NOTHING
    RETURNING id INTO review_id;
    
    RETURN review_id;
END;
$$ LANGUAGE plpgsql;

-- Create a new review
CREATE OR REPLACE FUNCTION cooktime.create_review(
    p_recipe_id uuid,
    p_owner_id uuid,
    p_rating integer,
    p_text text
)
RETURNS uuid AS $$
DECLARE
    review_id uuid;
BEGIN
    INSERT INTO cooktime.reviews (
        recipe_id,
        owner_id,
        rating,
        comment
    ) VALUES (
        p_recipe_id,
        p_owner_id,
        p_rating,
        p_text
    )
    RETURNING id INTO review_id;
    
    RETURN review_id;
END;
$$ LANGUAGE plpgsql;