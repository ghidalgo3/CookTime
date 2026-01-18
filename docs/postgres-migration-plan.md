# PostgreSQL Migration Plan: Replace Entity Framework

## Overview

Replace Entity Framework entirely with plain PostgreSQL schema and stored procedures using snake_case database objects, camelCase JSON properties, and stored procedures handling all data operations.

## Database Design Decisions

- **Schema**: All application tables in `cooktime` schema
- **Identity Tables**: Keep in `public` schema (ASP.NET Core Identity standard)
- **Naming Convention**: snake_case for database objects (tables, columns, functions)
- **JSON Properties**: camelCase for JSON in stored procedures and DTOs
- **Schema Qualification**: Always use fully qualified names (e.g., `cooktime.recipes`)
- **Connection String**: Read from `DATABASE_URL` environment variable, no search_path assumption
- **Error Handling**: Use PostgreSQL `RAISE EXCEPTION` for errors, catch in C# repositories
- **Transactions**: Managed within stored procedures for data consistency

## Migration Steps

### 1. Create Core Schema DDL Script

**File**: `src/Scripts/001_schema.sql`

**Contents**:

- `CREATE SCHEMA IF NOT EXISTS cooktime`
- Define `cooktime.unit` enum (tablespoon, teaspoon, cup, ounce, pound, gram, kilogram, milliliter, liter, fluid_ounce, pint, quart, gallon, count)
- Create 19 tables with snake_case names:
  - `cooktime.recipes` (renamed from MultiPartRecipes)
  - `cooktime.recipe_components` (renamed from RecipeComponents)
  - `cooktime.ingredient_requirements` (renamed from MultiPartIngredientRequirements)
  - `cooktime.recipe_steps` (renamed from MultiPartRecipeSteps)
  - `cooktime.ingredients`
  - `cooktime.categories`
  - `cooktime.category_recipe` (join table)
  - `cooktime.recipe_lists` (renamed from Carts)
  - `cooktime.recipe_requirements` (join table with quantity multiplier)
  - `cooktime.images`
  - `cooktime.reviews`
  - `cooktime.nutrition_facts` (new unified table)
- All primary keys (uuid or serial)
- All foreign keys fully qualified to `cooktime.table_name`
- Check constraints where applicable
- Indexes including GIN index on `cooktime.recipes.search_vector` for full-text search
- Keep `public."AspNetUsers"` and other Identity tables unchanged

**Key Table Schemas**:

**recipes**:

- id (uuid, PK)
- name (text, not null)
- description (text)
- cooking_minutes (double precision)
- servings (integer)
- prep_minutes (double precision)
- bake_temp_f (double precision)
- calories (integer)
- source (text)
- search_vector (tsvector) - generated column
- created_date (timestamptz)
- last_modified_date (timestamptz)
- owner_id (text, FK to public."AspNetUsers")

**recipe_components**:

- id (uuid, PK)
- name (text)
- position (integer)
- recipe_id (uuid, FK to cooktime.recipes)

**ingredient_requirements**:

- id (uuid, PK)
- ingredient_id (uuid, FK to cooktime.ingredients, nullable)
- recipe_component_id (uuid, FK to cooktime.recipe_components)
- unit (cooktime.unit enum)
- quantity (double precision)
- position (integer)
- description (text) - freeform text for imprecise ingredients

**recipe_steps**:

- id (serial, PK)
- recipe_component_id (uuid, FK to cooktime.recipe_components)
- instruction (text)

**recipe_lists**:

- id (uuid, PK)
- name (text, default 'List')
- description (text)
- creation_date (timestamp)
- is_public (boolean)
- owner_id (text, FK to public."AspNetUsers")

**recipe_requirements**:

- id (uuid, PK)
- recipe_list_id (uuid, FK to cooktime.recipe_lists)
- recipe_id (uuid, FK to cooktime.recipes)
- quantity (double precision) - scale multiplier for ingredient calculations

**nutrition_facts**:

- id (uuid, PK)
- source_ids (jsonb, not null) - e.g., `{"source": "usda_sr", "ndbNumber": 123}` or `{"source": "usda_branded", "gtinUpc": "012345"}`
- names (text[], not null)
- unit_mass (double precision, nullable)
- density (double precision, nullable)
- nutrition_data (jsonb, not null) - full nutrition information

**images**:

- id (uuid, PK)
- storage_url (text, not null) - Azure Blob Storage URL
- uploaded_date (timestamptz)
- static_image_name (text)
- recipe_id (uuid, FK to cooktime.recipes, nullable)
- ingredient_id (uuid, FK to cooktime.ingredients, nullable)

### 2. Define JSON Validation Functions

**File**: `src/Scripts/002_validation.sql`

**Functions**:

**`cooktime.validate_recipe_json(jsonb) RETURNS boolean`**

- Check camelCase properties: `name`, `components`, `cookingMinutes`, `servings`
- Validate `components` is array
- Validate each component has required fields
- `RAISE EXCEPTION` with descriptive messages for validation failures

**`cooktime.validate_component_json(jsonb) RETURNS boolean`**

- Check properties: `name`, `position`, `steps`, `ingredients`
- Validate arrays are properly structured

**`cooktime.validate_ingredient_requirement_json(jsonb) RETURNS boolean`**

- Check properties: `ingredientId`, `quantity`, `unit`
- Validate unit is valid enum value

**`cooktime.validate_recipe_list_json(jsonb) RETURNS boolean`**

- Check properties: `name`, `ownerId`

### 3. Create Write Stored Procedures

**File**: `src/Scripts/003_write_functions.sql`

**Functions**:

**`cooktime.create_recipe(recipe_json jsonb) RETURNS uuid`**

- BEGIN/COMMIT transaction
- Validate JSON with `cooktime.validate_recipe_json()`
- Extract recipe properties from JSON
- INSERT into `cooktime.recipes` RETURNING id
- Use CTEs to insert into:
  - `cooktime.recipe_components` (from `components` array)
  - `cooktime.ingredient_requirements` (from nested `ingredients` arrays)
  - `cooktime.recipe_steps` (from nested `steps` arrays)
- COMMIT and return recipe id

**`cooktime.update_recipe(recipe_id uuid, recipe_json jsonb)`**

- BEGIN transaction
- Validate recipe exists
- DELETE existing components/ingredients/steps (cascade)
- Re-insert with new data from JSON
- UPDATE recipe table fields
- COMMIT

**`cooktime.delete_recipe(recipe_id uuid)`**

- Handle cascade deletes for components, requirements, steps
- DELETE from `cooktime.recipes`

**`cooktime.create_or_update_nutrition_data(nutrition_json jsonb) RETURNS uuid`**

- Extract: `sourceIds`, `names`, `unitMass`, `density`, `nutritionData`
- UPSERT based on matching `source_ids` jsonb
- Return nutrition facts id

**`cooktime.create_ingredient(ingredient_json jsonb) RETURNS uuid`**

- Extract: `name`, `defaultServingSizeUnit`, `nutritionFactsId`
- INSERT into `cooktime.ingredients`

**`cooktime.create_recipe_list(list_json jsonb) RETURNS uuid`**

- Extract: `name`, `description`, `ownerId`, `isPublic`
- INSERT into `cooktime.recipe_lists`

**`cooktime.add_recipe_to_list(list_id uuid, recipe_id uuid, quantity_multiplier float)`**

- INSERT into `cooktime.recipe_requirements`
- Validate both list and recipe exist

### 4. Create Read Stored Procedures

**File**: `src/Scripts/004_read_functions.sql`

**Functions**:

**`cooktime.get_recipe_with_details(recipe_id uuid) RETURNS jsonb`**

- JOIN `cooktime.recipes`, `cooktime.recipe_components`, `cooktime.ingredient_requirements`, `cooktime.recipe_steps`
- Use `jsonb_build_object()` with camelCase keys
- Use `jsonb_agg()` to nest components containing steps and ingredients arrays
- Return single JSON document with complete recipe structure

**`cooktime.search_recipes_by_name(search_term text) RETURNS SETOF jsonb`**

- Use `cooktime.recipes.search_vector @@ plainto_tsquery('english', search_term)`
- Return array of recipe summary JSON objects

**`cooktime.search_recipes_by_ingredient(ingredient_id uuid) RETURNS SETOF jsonb`**

- JOIN through `cooktime.ingredient_requirements`
- Return recipes containing specified ingredient

**`cooktime.get_user_recipe_lists(user_id text) RETURNS SETOF jsonb`**

- SELECT from `cooktime.recipe_lists`
- Include recipe counts
- Return array of list summary JSON objects

**`cooktime.get_recipe_list_with_recipes(list_id uuid) RETURNS jsonb`**

- JOIN `cooktime.recipe_lists`, `cooktime.recipe_requirements`, `cooktime.recipes`
- Include quantity multipliers for each recipe
- Return complete list with nested recipes

**`cooktime.get_recipe_images(recipe_id uuid) RETURNS SETOF jsonb`**

- SELECT from `cooktime.images` WHERE recipe_id matches
- Return camelCase JSON array

### 5. Alter Images Table Schema

**File**: `src/Scripts/005_images.sql`

**Operations**:

- `ALTER TABLE cooktime.images DROP COLUMN IF EXISTS image_data CASCADE`
- `ADD COLUMN IF NOT EXISTS storage_url text NOT NULL DEFAULT ''`

**Functions**:

**`cooktime.create_image(image_json jsonb) RETURNS uuid`**

- Extract: `storageUrl`, `recipeId`, `ingredientId`, `staticImageName`
- INSERT into `cooktime.images`

**Note**: Actual Azure Blob upload of existing image byte data happens in separate operational migration outside SQL scripts.

### 6. Define C# DTOs and Contracts

**Location**: `src/Models/Contracts/`

**Files to Create**:

**`RecipeCreateDto.cs`**

```csharp
public class RecipeCreateDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("components")]
    public List<ComponentDto> Components { get; set; }
    
    [JsonPropertyName("cookingMinutes")]
    public double? CookingMinutes { get; set; }
    
    [JsonPropertyName("servings")]
    public int? Servings { get; set; }
    
    [JsonPropertyName("prepMinutes")]
    public double? PrepMinutes { get; set; }
    
    [JsonPropertyName("bakeTempF")]
    public double? BakeTempF { get; set; }
    
    [JsonPropertyName("source")]
    public string? Source { get; set; }
}
```

**`RecipeDetailDto.cs`**

- All recipe data including nested components with full details

**`ComponentDto.cs`**

```csharp
public class ComponentDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("position")]
    public int Position { get; set; }
    
    [JsonPropertyName("steps")]
    public List<RecipeStepDto> Steps { get; set; }
    
    [JsonPropertyName("ingredients")]
    public List<IngredientRequirementDto> Ingredients { get; set; }
}
```

**`RecipeStepDto.cs`**

```csharp
public class RecipeStepDto
{
    [JsonPropertyName("instruction")]
    public string Instruction { get; set; }
}
```

**`IngredientRequirementDto.cs`**

```csharp
public class IngredientRequirementDto
{
    [JsonPropertyName("ingredientId")]
    public Guid? IngredientId { get; set; }
    
    [JsonPropertyName("quantity")]
    public double Quantity { get; set; }
    
    [JsonPropertyName("unit")]
    public string Unit { get; set; }
    
    [JsonPropertyName("position")]
    public int Position { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
```

**`RecipeListDto.cs`**
**`RecipeRequirementDto.cs`** (with `Quantity` multiplier)
**`NutritionDataDto.cs`**

### 7. Create Repository Classes

**Location**: `src/Services/Repositories/`

**Files to Create**:

**`IRecipeRepository.cs`** (interface)
**`RecipeRepository.cs`**

- Inject `IConfiguration` for connection string
- Methods:
  - `Task<Guid> CreateAsync(RecipeCreateDto dto)`
    - Serialize to JSON
    - Call `SELECT cooktime.create_recipe(@json::jsonb)`
    - Return recipe id
  - `Task<RecipeDetailDto?> GetDetailAsync(Guid id)`
    - Call `SELECT * FROM cooktime.get_recipe_with_details(@id)`
    - Deserialize JSON result
  - `Task<List<RecipeDetailDto>> SearchByNameAsync(string searchTerm)`
  - `Task UpdateAsync(Guid id, RecipeCreateDto dto)`
  - `Task DeleteAsync(Guid id)`
- Catch `PostgresException` and translate to domain-specific exceptions
- Use `await using var connection = new NpgsqlConnection(connectionString)`

**`IIngredientRepository.cs`** (interface)
**`IngredientRepository.cs`**

**`IRecipeListRepository.cs`** (interface)
**`RecipeListRepository.cs`** (replaces CartRepository/CartService)

**`INutritionRepository.cs`** (interface)
**`NutritionRepository.cs`**

**`IImageRepository.cs`** (interface)
**`ImageRepository.cs`**

### 8. Update Controllers

**Location**: `src/Controllers/`

**Changes**:

**Rename Files**:

- `MultiPartRecipeController.cs` → `RecipeController.cs`
- `CartController.cs` → `RecipeListController.cs`

**Update All Controllers**:

- Remove `DbContext` injection
- Inject appropriate repository interfaces
- Replace all LINQ queries with repository method calls
- Remove all `.Include()`, `.ThenInclude()`, `.Where()`, `.FirstOrDefaultAsync()`, `.SaveChangesAsync()` patterns
- Use DTOs for request/response instead of EF entities

**Controllers to Update**:

- `RecipeController.cs` (renamed)
- `IngredientController.cs`
- `RecipeListController.cs` (renamed)
- `IImageController.cs`
- `AccountController.cs`
- `SEOController.cs`

### 9. Remove Entity Framework Dependencies

**Actions**:

1. **Delete Migrations folder**: `src/Migrations/` (all `*Migration.cs`, `*Migration.Designer.cs` files, `ApplicationDbContextModelSnapshot.cs`)

2. **Remove NuGet packages** from `src/babe-algorithms.csproj`:
   - `Microsoft.EntityFrameworkCore`
   - `Microsoft.EntityFrameworkCore.Design`
   - `Microsoft.EntityFrameworkCore.Tools`
   - `Npgsql.EntityFrameworkCore.PostgreSQL`

3. **Delete DbContext class file**: Remove `ApplicationDbContext.cs`

4. **Update `Startup.cs`/`Program.cs`**:
   - Remove `services.AddDbContext<ApplicationDbContext>()`
   - Register repositories:

     ```csharp
     services.AddScoped<IRecipeRepository, RecipeRepository>();
     services.AddScoped<IIngredientRepository, IngredientRepository>();
     services.AddScoped<IRecipeListRepository, RecipeListRepository>();
     services.AddScoped<INutritionRepository, NutritionRepository>();
     services.AddScoped<IImageRepository, ImageRepository>();
     ```

   - Keep entire ASP.NET Core Identity configuration:

     ```csharp
     services.AddIdentity<ApplicationUser, IdentityRole>()
         .AddEntityFrameworkStores<IdentityDbContext>()
         .AddDefaultTokenProviders();
     ```

5. **Keep Identity Tables**: `public."AspNetUsers"`, `public."AspNetRoles"`, `public."AspNetUserRoles"`, `public."AspNetUserClaims"`, `public."AspNetUserLogins"`, `public."AspNetUserTokens"`, `public."AspNetRoleClaims"`

### 10. Create Migration Runner System

**Files**:

**`src/Scripts/000_migration_tracker.sql`**

```sql
CREATE SCHEMA IF NOT EXISTS cooktime;

CREATE TABLE IF NOT EXISTS cooktime.schema_migrations (
    id serial PRIMARY KEY,
    script_name text UNIQUE NOT NULL,
    applied_at timestamptz DEFAULT now(),
    checksum text
);
```

**`src/Scripts/run_migrations.sh`**

```bash
#!/bin/bash
set -e

# Read connection string from environment
DB_URL="${DATABASE_URL}"

if [ -z "$DB_URL" ]; then
    echo "Error: DATABASE_URL environment variable not set"
    exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Initialize migration tracker
psql "$DB_URL" -f "$SCRIPT_DIR/000_migration_tracker.sql"

# Get list of SQL files in order
for sql_file in "$SCRIPT_DIR"/*.sql; do
    filename=$(basename "$sql_file")
    
    # Skip the migration tracker itself
    if [ "$filename" = "000_migration_tracker.sql" ]; then
        continue
    fi
    
    # Check if already applied
    applied=$(psql "$DB_URL" -t -c "SELECT COUNT(*) FROM cooktime.schema_migrations WHERE script_name = '$filename'")
    
    if [ "$applied" -eq 0 ]; then
        echo "Applying migration: $filename"
        
        # Calculate checksum
        checksum=$(md5sum "$sql_file" | awk '{print $1}')
        
        # Execute migration
        psql "$DB_URL" -f "$sql_file"
        
        # Record in tracker
        psql "$DB_URL" -c "INSERT INTO cooktime.schema_migrations (script_name, checksum) VALUES ('$filename', '$checksum')"
        
        echo "✓ Applied: $filename"
    else
        echo "⊘ Skipped (already applied): $filename"
    fi
done

echo "All migrations completed successfully"
```

**Usage**:

```bash
export DATABASE_URL="postgresql://user:password@localhost:5432/dbname"
chmod +x src/Scripts/run_migrations.sh
./src/Scripts/run_migrations.sh
```

**Function Updates**: All numbered SQL files after 001 should use pattern:

```sql
-- Drop existing functions if schema changes
DROP FUNCTION IF EXISTS cooktime.create_recipe CASCADE;
DROP FUNCTION IF EXISTS cooktime.update_recipe CASCADE;

-- Create new versions
CREATE FUNCTION cooktime.create_recipe(recipe_json jsonb) RETURNS uuid AS $$
...
$$ LANGUAGE plpgsql;
```

## Tables Summary

### Tables Created (19 in `cooktime` schema)

1. `recipes` (renamed from MultiPartRecipes)
2. `recipe_components` (renamed from RecipeComponents)
3. `ingredient_requirements` (renamed from MultiPartIngredientRequirements)
4. `recipe_steps` (renamed from MultiPartRecipeSteps)
5. `ingredients`
6. `categories`
7. `category_recipe` (join table)
8. `recipe_lists` (renamed from Carts)
9. `recipe_requirements` (join table with quantity multiplier)
10. `images`
11. `reviews`
12. `nutrition_facts` (new unified table)
13-19. Plus other supporting tables

### Tables Kept (7 in `public` schema)

1. `AspNetUsers`
2. `AspNetRoles`
3. `AspNetUserRoles`
4. `AspNetUserClaims`
5. `AspNetUserLogins`
6. `AspNetUserTokens`
7. `AspNetRoleClaims`

### Tables Removed (8 legacy tables)

1. `Recipes` (legacy simple recipes)
2. `RecipeSteps` (legacy)
3. `IngredientRequirements` (legacy)
4. `CategoryRecipe` (legacy join)
5. `Events` (unused feature)
6. `Carts` (renamed to recipe_lists)
7. `CartIngredients` (removed, recipes only in lists)
8. `StandardReferenceNutritionData` (consolidated to nutrition_facts)
9. `BrandedNutritionData` (consolidated to nutrition_facts)

## Key Changes Summary

- **Entity Naming**: MultiPartRecipe → Recipe (simplified)
- **Purpose Clarity**: Cart → RecipeList
- **Nutrition Consolidation**: Two tables → Single `nutrition_facts` with jsonb
- **Image Storage**: Byte arrays → Azure Blob URLs
- **Data Access**: Entity Framework ORM → Raw Npgsql + stored procedures
- **Schema Management**: EF Migrations → Numbered SQL scripts with tracking table
- **Transactions**: Application-managed → Stored procedure-managed
- **Validation**: EF attributes → PostgreSQL validation functions
- **Relationships**: Navigation properties → Explicit JOINs in stored procedures

## Testing Strategy

Create separate test database with same `cooktime` schema for integration tests. Test against realistic PostgreSQL instance without affecting production data.

## Migration Rollback Strategy

Rely on database backups for rollback during initial development. No down migration scripts initially - add later if needed for production scenarios.

## Next Steps

1. Define specification for exporting recipes from database
2. Implement core schema DDL script (001_schema.sql)
3. Implement validation functions (002_validation.sql)
4. Implement write stored procedures (003_write_functions.sql)
5. Implement read stored procedures (004_read_functions.sql)
6. Implement image schema changes (005_images.sql)
7. Create C# DTOs and contracts
8. Implement repository classes
9. Update controllers
10. Remove EF dependencies
11. Test with test database
12. Migrate production data
