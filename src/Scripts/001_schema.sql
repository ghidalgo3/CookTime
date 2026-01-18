-- PostgreSQL Migration: Core Schema
-- Creates the cooktime schema with all tables

-- Users table (maps external OAuth identities to internal user ID)
CREATE TABLE IF NOT EXISTS cooktime.users (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    provider text NOT NULL,           -- 'google', 'apple', 'facebook', etc.
    provider_user_id text NOT NULL,   -- The 'sub' claim from the OAuth provider
    email text,                       -- Optional, for display/contact purposes
    display_name text,
    roles text[] NOT NULL DEFAULT ARRAY['User'],
    created_date timestamptz DEFAULT now(),
    last_login_date timestamptz DEFAULT now(),
    UNIQUE (provider, provider_user_id)
);

CREATE INDEX IF NOT EXISTS idx_users_provider_user_id ON cooktime.users(provider, provider_user_id);
CREATE INDEX IF NOT EXISTS idx_users_email ON cooktime.users(email);
CREATE UNIQUE INDEX IF NOT EXISTS idx_users_display_name_unique ON cooktime.users(display_name) WHERE display_name IS NOT NULL;

-- Create unit enum type
DO $$ BEGIN
    CREATE TYPE cooktime.unit AS ENUM (
        -- Volume
        'tablespoon',
        'teaspoon', 
        'milliliter',
        'cup',
        'fluid_ounce',
        'pint',
        'quart',
        'gallon',
        'liter',
        -- Count
        'count',
        -- Mass
        'ounce',
        'pound',
        'milligram',
        'gram',
        'kilogram'
    );
EXCEPTION
    WHEN duplicate_object THEN NULL;
END $$;

-- Enable extensions
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Recipes table
CREATE TABLE IF NOT EXISTS cooktime.recipes (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name text NOT NULL,
    description text,
    cooking_minutes double precision,
    servings integer,
    prep_minutes double precision,
    calories integer,
    source text,
    search_vector tsvector GENERATED ALWAYS AS (
        to_tsvector('english', coalesce(name, '') || ' ' || coalesce(description, ''))
    ) STORED,
    created_date timestamptz DEFAULT now(),
    last_modified_date timestamptz DEFAULT now(),
    owner_id uuid REFERENCES cooktime.users(id)
);

-- Create GIN index for full-text search
CREATE INDEX IF NOT EXISTS idx_recipes_search_vector ON cooktime.recipes USING gin(search_vector);
CREATE INDEX IF NOT EXISTS idx_recipes_owner_id ON cooktime.recipes(owner_id);

-- Recipe components table
CREATE TABLE IF NOT EXISTS cooktime.recipe_components (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name text,
    position integer NOT NULL,
    steps text[] DEFAULT '{}',  -- Array of instruction strings, order = step order
    recipe_id uuid NOT NULL REFERENCES cooktime.recipes(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_recipe_components_recipe_id ON cooktime.recipe_components(recipe_id);

-- Ingredients table
CREATE TABLE IF NOT EXISTS cooktime.ingredients (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name text NOT NULL,
    expected_unit_mass_kg double precision DEFAULT 0.1,
    nutrition_facts_id uuid
);

CREATE INDEX IF NOT EXISTS idx_ingredients_name ON cooktime.ingredients(name);
CREATE INDEX IF NOT EXISTS idx_ingredients_nutrition_facts_id ON cooktime.ingredients(nutrition_facts_id);

CREATE TABLE IF NOT EXISTS cooktime.ingredient_requirements (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    ingredient_id uuid REFERENCES cooktime.ingredients(id),
    recipe_component_id uuid NOT NULL REFERENCES cooktime.recipe_components(id) ON DELETE CASCADE,
    unit cooktime.unit,
    quantity double precision NOT NULL,
    position integer NOT NULL,
    description text
);

CREATE INDEX IF NOT EXISTS idx_ingredient_requirements_ingredient_id ON cooktime.ingredient_requirements(ingredient_id);
CREATE INDEX IF NOT EXISTS idx_ingredient_requirements_recipe_component_id ON cooktime.ingredient_requirements(recipe_component_id);

-- Categories table
CREATE TABLE IF NOT EXISTS cooktime.categories (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name text UNIQUE NOT NULL
);

-- Category-Recipe join table
CREATE TABLE IF NOT EXISTS cooktime.category_recipe (
    category_id uuid NOT NULL REFERENCES cooktime.categories(id) ON DELETE CASCADE,
    recipe_id uuid NOT NULL REFERENCES cooktime.recipes(id) ON DELETE CASCADE,
    PRIMARY KEY (category_id, recipe_id)
);

CREATE INDEX IF NOT EXISTS idx_category_recipe_recipe_id ON cooktime.category_recipe(recipe_id);

-- Recipe lists table (renamed from Carts)
CREATE TABLE IF NOT EXISTS cooktime.recipe_lists (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name text DEFAULT 'List',
    description text,
    creation_date timestamptz DEFAULT now(),
    is_public boolean DEFAULT false,
    owner_id uuid NOT NULL REFERENCES cooktime.users(id)
);

CREATE INDEX IF NOT EXISTS idx_recipe_lists_owner_id ON cooktime.recipe_lists(owner_id);

-- Recipe requirements table (join table with quantity multiplier)
CREATE TABLE IF NOT EXISTS cooktime.recipe_requirements (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    recipe_list_id uuid NOT NULL REFERENCES cooktime.recipe_lists(id) ON DELETE CASCADE,
    recipe_id uuid NOT NULL REFERENCES cooktime.recipes(id) ON DELETE CASCADE,
    quantity double precision NOT NULL DEFAULT 1.0
);

CREATE INDEX IF NOT EXISTS idx_recipe_requirements_recipe_list_id ON cooktime.recipe_requirements(recipe_list_id);
CREATE INDEX IF NOT EXISTS idx_recipe_requirements_recipe_id ON cooktime.recipe_requirements(recipe_id);

-- Images table
CREATE TABLE IF NOT EXISTS cooktime.images (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    storage_url text NOT NULL,
    uploaded_date timestamptz DEFAULT now(),
    static_image_name text,
    recipe_id uuid REFERENCES cooktime.recipes(id) ON DELETE CASCADE,
    ingredient_id uuid REFERENCES cooktime.ingredients(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_images_recipe_id ON cooktime.images(recipe_id);
CREATE INDEX IF NOT EXISTS idx_images_ingredient_id ON cooktime.images(ingredient_id);

-- Reviews table
CREATE TABLE IF NOT EXISTS cooktime.reviews (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    created_date timestamptz DEFAULT now(),
    last_modified_date timestamptz DEFAULT now(),
    owner_id uuid NOT NULL REFERENCES cooktime.users(id),
    recipe_id uuid REFERENCES cooktime.recipes(id) ON DELETE CASCADE,
    rating integer NOT NULL CHECK (rating >= 1 AND rating <= 5),
    comment text
);

CREATE INDEX IF NOT EXISTS idx_reviews_owner_id ON cooktime.reviews(owner_id);
CREATE INDEX IF NOT EXISTS idx_reviews_recipe_id ON cooktime.reviews(recipe_id);

-- Nutrition facts table (consolidated from StandardReferenceNutritionData and BrandedNutritionData)
CREATE TABLE IF NOT EXISTS cooktime.nutrition_facts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    source_ids jsonb NOT NULL,
    names text[] NOT NULL,
    unit_mass double precision,
    density double precision,
    nutrition_data jsonb NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_nutrition_facts_source_ids ON cooktime.nutrition_facts USING gin(source_ids);

-- Add foreign key from ingredients to nutrition_facts (only if not exists)
DO $$ BEGIN
    ALTER TABLE cooktime.ingredients 
    ADD CONSTRAINT fk_ingredients_nutrition_facts 
    FOREIGN KEY (nutrition_facts_id) REFERENCES cooktime.nutrition_facts(id);
EXCEPTION
    WHEN duplicate_object THEN NULL;
END $$;

-- Create trigger to update last_modified_date
CREATE OR REPLACE FUNCTION cooktime.update_last_modified_date()
RETURNS TRIGGER AS $$
BEGIN
    NEW.last_modified_date = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS update_recipes_last_modified ON cooktime.recipes;
CREATE TRIGGER update_recipes_last_modified
BEFORE UPDATE ON cooktime.recipes
FOR EACH ROW
EXECUTE FUNCTION cooktime.update_last_modified_date();

DROP TRIGGER IF EXISTS update_reviews_last_modified ON cooktime.reviews;
CREATE TRIGGER update_reviews_last_modified
BEFORE UPDATE ON cooktime.reviews
FOR EACH ROW
EXECUTE FUNCTION cooktime.update_last_modified_date();
