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
BEGIN
    -- Validate input
    PERFORM cooktime.validate_recipe_json(recipe_json);
    
    -- Begin transaction (implicit in function)
    
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
            ARRAY(SELECT jsonb_array_elements_text(
                COALESCE(
                    (SELECT jsonb_agg(s->>'instruction') FROM jsonb_array_elements(component_item->'steps') s),
                    '[]'::jsonb
                )
            )),
            new_recipe_id
        )
        RETURNING id INTO component_id;
        
        -- Insert ingredient requirements for this component
        FOR ingredient_item IN SELECT * FROM jsonb_array_elements(component_item->'ingredients')
        LOOP
            PERFORM cooktime.validate_ingredient_requirement_json(ingredient_item);
            
            INSERT INTO cooktime.ingredient_requirements (
                ingredient_id,
                recipe_component_id,
                unit,
                quantity,
                position,
                description
            ) VALUES (
                (ingredient_item->>'ingredientId')::uuid,
                component_id,
                (ingredient_item->>'unit')::cooktime.unit,
                (ingredient_item->>'quantity')::double precision,
                (ingredient_item->>'position')::integer,
                ingredient_item->>'description'
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
            ARRAY(SELECT jsonb_array_elements_text(
                COALESCE(
                    (SELECT jsonb_agg(s->>'instruction') FROM jsonb_array_elements(component_item->'steps') s),
                    '[]'::jsonb
                )
            )),
            p_recipe_id
        )
        RETURNING id INTO component_id;
        
        FOR ingredient_item IN SELECT * FROM jsonb_array_elements(component_item->'ingredients')
        LOOP
            PERFORM cooktime.validate_ingredient_requirement_json(ingredient_item);
            
            INSERT INTO cooktime.ingredient_requirements (
                ingredient_id,
                recipe_component_id,
                unit,
                quantity,
                position,
                description
            ) VALUES (
                (ingredient_item->>'ingredientId')::uuid,
                component_id,
                (ingredient_item->>'unit')::cooktime.unit,
                (ingredient_item->>'quantity')::double precision,
                (ingredient_item->>'position')::integer,
                ingredient_item->>'description'
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
            nutrition_data = nutrition_json->'nutritionData'
        WHERE id = existing_id;
        
        RETURN existing_id;
    ELSE
        -- Insert new
        INSERT INTO cooktime.nutrition_facts (
            source_ids,
            names,
            unit_mass,
            density,
            nutrition_data
        ) VALUES (
            nutrition_json->'sourceIds',
            ARRAY(SELECT jsonb_array_elements_text(nutrition_json->'names')),
            (nutrition_json->>'unitMass')::double precision,
            (nutrition_json->>'density')::double precision,
            nutrition_json->'nutritionData'
        )
        RETURNING id INTO nutrition_id;
        
        RETURN nutrition_id;
    END IF;
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
