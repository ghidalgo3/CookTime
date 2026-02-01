-- Migration: Update recipe functions to handle nested ingredient objects with isNew flag
-- This allows creating new ingredients on-the-fly when creating/updating recipes

-- Update the ingredient requirement validation to accept the new nested structure
CREATE OR REPLACE FUNCTION cooktime.validate_ingredient_requirement_json(requirement_json jsonb)
RETURNS boolean AS $$
BEGIN
    -- Check required fields
    IF NOT (requirement_json ? 'quantity' AND requirement_json ? 'position') THEN
        RAISE EXCEPTION 'Ingredient requirement JSON must contain "quantity" and "position" fields';
    END IF;
    
    -- Check for nested ingredient object
    IF NOT (requirement_json ? 'ingredient') THEN
        RAISE EXCEPTION 'Ingredient requirement JSON must contain "ingredient" object';
    END IF;
    
    -- Check ingredient object has required fields
    IF NOT (requirement_json->'ingredient' ? 'id' AND requirement_json->'ingredient' ? 'name') THEN
        RAISE EXCEPTION 'Ingredient object must contain "id" and "name" fields';
    END IF;
    
    -- Check quantity is a number
    IF jsonb_typeof(requirement_json->'quantity') != 'number' THEN
        RAISE EXCEPTION 'Ingredient requirement "quantity" must be a number';
    END IF;
    
    -- Check position is a number
    IF jsonb_typeof(requirement_json->'position') != 'number' THEN
        RAISE EXCEPTION 'Ingredient requirement "position" must be a number';
    END IF;
    
    -- Check unit if provided
    IF requirement_json ? 'unit' AND requirement_json->>'unit' IS NOT NULL AND jsonb_typeof(requirement_json->'unit') != 'string' THEN
        RAISE EXCEPTION 'Ingredient requirement "unit" must be a string';
    END IF;
    
    RETURN true;
END;
$$ LANGUAGE plpgsql;

-- Helper function to ensure ingredient exists (creates if isNew=true)
CREATE OR REPLACE FUNCTION cooktime.ensure_ingredient_exists(ingredient_json jsonb)
RETURNS uuid AS $$
DECLARE
    v_ingredient_id uuid;
    v_is_new boolean;
BEGIN
    v_ingredient_id := (ingredient_json->>'id')::uuid;
    v_is_new := COALESCE((ingredient_json->>'isNew')::boolean, false);
    
    -- Check if ingredient exists
    IF EXISTS (SELECT 1 FROM cooktime.ingredients WHERE id = v_ingredient_id) THEN
        RETURN v_ingredient_id;
    END IF;
    
    -- If ingredient doesn't exist and isNew is true, create it
    IF v_is_new THEN
        INSERT INTO cooktime.ingredients (id, name)
        VALUES (
            v_ingredient_id,
            ingredient_json->>'name'
        );
        RETURN v_ingredient_id;
    ELSE
        -- Ingredient doesn't exist and isNew is false - this is an error
        RAISE EXCEPTION 'Ingredient with id % does not exist and isNew is false', v_ingredient_id;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Update create_recipe to handle nested ingredient objects
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

-- Update update_recipe to handle nested ingredient objects
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
