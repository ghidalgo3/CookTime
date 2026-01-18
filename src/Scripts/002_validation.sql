-- PostgreSQL Migration: JSON Validation Functions
-- Provides validation for JSON inputs to stored procedures

-- Validate recipe JSON
CREATE OR REPLACE FUNCTION cooktime.validate_recipe_json(recipe_json jsonb)
RETURNS boolean AS $$
BEGIN
    -- Check required fields exist
    IF NOT (recipe_json ? 'name' AND recipe_json ? 'components') THEN
        RAISE EXCEPTION 'Recipe JSON must contain "name" and "components" fields';
    END IF;
    
    -- Check name is a string
    IF jsonb_typeof(recipe_json->'name') != 'string' THEN
        RAISE EXCEPTION 'Recipe "name" must be a string';
    END IF;
    
    -- Check components is an array
    IF jsonb_typeof(recipe_json->'components') != 'array' THEN
        RAISE EXCEPTION 'Recipe "components" must be an array';
    END IF;
    
    -- Check optional numeric fields
    IF recipe_json ? 'cookingMinutes' AND jsonb_typeof(recipe_json->'cookingMinutes') NOT IN ('number', 'null') THEN
        RAISE EXCEPTION 'Recipe "cookingMinutes" must be a number or null';
    END IF;
    
    IF recipe_json ? 'servings' AND jsonb_typeof(recipe_json->'servings') NOT IN ('number', 'null') THEN
        RAISE EXCEPTION 'Recipe "servings" must be a number or null';
    END IF;
    
    RETURN true;
END;
$$ LANGUAGE plpgsql;

-- Validate component JSON
CREATE OR REPLACE FUNCTION cooktime.validate_component_json(component_json jsonb)
RETURNS boolean AS $$
BEGIN
    -- Check required fields
    IF NOT (component_json ? 'position' AND component_json ? 'steps' AND component_json ? 'ingredients') THEN
        RAISE EXCEPTION 'Component JSON must contain "position", "steps", and "ingredients" fields';
    END IF;
    
    -- Check position is a number
    IF jsonb_typeof(component_json->'position') != 'number' THEN
        RAISE EXCEPTION 'Component "position" must be a number';
    END IF;
    
    -- Check steps is an array
    IF jsonb_typeof(component_json->'steps') != 'array' THEN
        RAISE EXCEPTION 'Component "steps" must be an array';
    END IF;
    
    -- Check ingredients is an array
    IF jsonb_typeof(component_json->'ingredients') != 'array' THEN
        RAISE EXCEPTION 'Component "ingredients" must be an array';
    END IF;
    
    RETURN true;
END;
$$ LANGUAGE plpgsql;

-- Validate ingredient requirement JSON
CREATE OR REPLACE FUNCTION cooktime.validate_ingredient_requirement_json(requirement_json jsonb)
RETURNS boolean AS $$
BEGIN
    -- Check required fields
    IF NOT (requirement_json ? 'quantity' AND requirement_json ? 'position') THEN
        RAISE EXCEPTION 'Ingredient requirement JSON must contain "quantity" and "position" fields';
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
    IF requirement_json ? 'unit' AND jsonb_typeof(requirement_json->'unit') != 'string' THEN
        RAISE EXCEPTION 'Ingredient requirement "unit" must be a string';
    END IF;
    
    RETURN true;
END;
$$ LANGUAGE plpgsql;

-- Validate recipe list JSON
CREATE OR REPLACE FUNCTION cooktime.validate_recipe_list_json(list_json jsonb)
RETURNS boolean AS $$
BEGIN
    -- Check required fields
    IF NOT (list_json ? 'name' AND list_json ? 'ownerId') THEN
        RAISE EXCEPTION 'Recipe list JSON must contain "name" and "ownerId" fields';
    END IF;
    
    -- Check name is a string
    IF jsonb_typeof(list_json->'name') != 'string' THEN
        RAISE EXCEPTION 'Recipe list "name" must be a string';
    END IF;
    
    -- Check ownerId is a string
    IF jsonb_typeof(list_json->'ownerId') != 'string' THEN
        RAISE EXCEPTION 'Recipe list "ownerId" must be a string';
    END IF;
    
    RETURN true;
END;
$$ LANGUAGE plpgsql;

-- Validate ingredient JSON
CREATE OR REPLACE FUNCTION cooktime.validate_ingredient_json(ingredient_json jsonb)
RETURNS boolean AS $$
BEGIN
    -- Check required fields
    IF NOT (ingredient_json ? 'name') THEN
        RAISE EXCEPTION 'Ingredient JSON must contain "name" field';
    END IF;
    
    -- Check name is a string
    IF jsonb_typeof(ingredient_json->'name') != 'string' THEN
        RAISE EXCEPTION 'Ingredient "name" must be a string';
    END IF;
    
    RETURN true;
END;
$$ LANGUAGE plpgsql;
