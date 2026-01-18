# PostgreSQL Migration Implementation Status

## ✅ Completed

### 1. Migration Infrastructure

- [x] **000_migration_tracker.sql** - Migration tracking table
- [x] **run_migrations.sh** - Bash script to execute migrations in order with checksum tracking

### 2. Database Schema (SQL Scripts)

- [x] **001_schema.sql** - Complete schema with 12 tables in `cooktime` schema
  - recipes, recipe_components, ingredient_requirements, recipe_steps
  - ingredients, categories, category_recipe
  - recipe_lists, recipe_requirements
  - images, reviews, nutrition_facts
  - Full-text search with tsvector + GIN indexes
  - Triggers for last_modified_date
  
- [x] **002_validation.sql** - JSON validation functions
  - validate_recipe_json, validate_component_json
  - validate_ingredient_requirement_json, validate_recipe_list_json
  - validate_ingredient_json
  
- [x] **003_write_functions.sql** - Write operations
  - create_recipe (with CTEs for components/steps/ingredients)
  - update_recipe (delete and recreate pattern)
  - delete_recipe
  - create_or_update_nutrition_data (UPSERT based on source_ids)
  - create_ingredient
  - create_recipe_list, add_recipe_to_list
  - create_category
  
- [x] **004_read_functions.sql** - Read operations returning JSON
  - get_recipe_with_details (nested JSON with jsonb_agg)
  - search_recipes_by_name (full-text search)
  - search_recipes_by_ingredient
  - get_recipes (paginated)
  - get_user_recipe_lists
  - get_recipe_list_with_recipes
  - get_recipe_images
  - get_ingredient (with nutrition facts)
  - search_ingredients (trigram similarity)
  - get_recipe_reviews
  - get_categories
  
- [x] **005_images.sql** - Image storage migration
  - Add storage_url column for Azure Blob URLs
  - create_image, get_ingredient_images functions

### 3. C# Data Transfer Objects (DTOs)

Created 10 DTO files in `/src/Models/Contracts/` with `[JsonPropertyName]` attributes:

- [x] **RecipeCreateDto.cs** - Recipe create/update DTOs
- [x] **RecipeDetailDto.cs** - Recipe detail and summary DTOs
- [x] **ComponentDto.cs** - Component DTOs
- [x] **IngredientRequirementDto.cs** - Ingredient requirement DTO
- [x] **RecipeStepDto.cs** - Recipe step DTO
- [x] **IngredientDto.cs** - Ingredient DTOs
- [x] **RecipeListDto.cs** - Recipe list DTOs
- [x] **CategoryDto.cs** - Category DTOs
- [x] **NutritionDataDto.cs** - Nutrition data DTO
- [x] **ReviewDto.cs** - Review DTOs

### 4. Repository Pattern Implementation

Created 5 repositories in `/src/Services/Repositories/`:

- [x] **IRecipeRepository** / **RecipeRepository** - Recipe CRUD operations
- [x] **IIngredientRepository** / **IngredientRepository** - Ingredient operations
- [x] **IRecipeListRepository** / **RecipeListRepository** - Recipe list operations
- [x] **ICategoryRepository** / **CategoryRepository** - Category operations
- [x] **IReviewRepository** / **ReviewRepository** - Review operations

All repositories:

- Use `NpgsqlDataSource` for connection management
- Call stored procedures defined in SQL scripts
- Deserialize JSON results to DTOs with camelCase property naming
- Handle null results appropriately

### 5. Dependency Injection Setup

- [x] Updated **Program.cs** to register `NpgsqlDataSource` as singleton
- [x] Registered all 5 repository interfaces as scoped services
- [x] Added DATABASE_URL environment variable fallback

---

## ⏳ Remaining Work

### 6. Update Controllers

Need to refactor controllers to use repositories instead of DbContext:

#### High Priority Controllers

- [ ] **MultiPartRecipeController.cs** → Rename to **RecipeController.cs**
  - Remove `ApplicationDbContext` injection
  - Inject `IRecipeRepository`, `ICategoryRepository`, `IReviewRepository`
  - Replace LINQ queries with repository methods
  - Update action methods to use DTOs
  
- [ ] **CartController.cs** → Rename to **RecipeListController.cs**
  - Remove `ApplicationDbContext` injection
  - Inject `IRecipeListRepository`
  - Replace LINQ queries with repository methods
  - Update action methods to use DTOs
  
- [ ] **IngredientController.cs**
  - Remove `ApplicationDbContext` injection
  - Inject `IIngredientRepository`
  - Replace LINQ queries with repository methods
  - Update action methods to use DTOs

#### Lower Priority Controllers

- [ ] **CategoryController.cs** - Update to use `ICategoryRepository`
- [ ] **IImageController.cs** - May need updates if it queries recipes/ingredients

### 7. Remove Entity Framework

- [ ] Delete entire **/Migrations/** folder (70+ migration files)
- [ ] Remove EF NuGet packages from **babe-algorithms.csproj**:
  - Microsoft.EntityFrameworkCore
  - Microsoft.EntityFrameworkCore.Design
  - Npgsql.EntityFrameworkCore.PostgreSQL
- [ ] Delete **ApplicationDbContext.cs** class (or keep minimal version for Identity only)
- [ ] Remove `AddDbContext` call from Program.cs (keep only for Identity if needed)
- [ ] Remove all EF-related code from `ConfigureDatabase` method in Program.cs

### 8. Testing & Data Migration

- [ ] Set DATABASE_URL environment variable
- [ ] Run `./src/Scripts/run_migrations.sh` to apply all migrations
- [ ] Export existing data using: `dotnet run export`
- [ ] Write data import script to load NDJSON into PostgreSQL
- [ ] Test all API endpoints with new repository implementation
- [ ] Verify full-text search functionality
- [ ] Verify image URL handling

### 9. Cleanup & Documentation

- [ ] Update README.md with new architecture
- [ ] Document stored procedure usage
- [ ] Remove obsolete model classes if any
- [ ] Update API documentation

---

## Migration Execution Steps

1. **Backup existing database**

   ```bash
   pg_dump $DATABASE_URL > backup_before_migration.sql
   ```

2. **Run migrations**

   ```bash
   export DATABASE_URL="postgresql://user:pass@host:port/dbname"
   ./src/Scripts/run_migrations.sh
   ```

3. **Export data from EF**

   ```bash
   dotnet run export
   ```

   Creates NDJSON files: `recipes.ndjson`, `ingredients.ndjson`, `images.ndjson`

4. **Import data** (script TBD)
   - Parse NDJSON files
   - Call stored procedures to insert data
   - Handle foreign key relationships

5. **Update controllers** (in progress)
   - Replace DbContext with repositories
   - Update to use DTOs

6. **Remove EF dependencies**
   - Delete migrations
   - Remove packages
   - Clean up Program.cs

7. **Test thoroughly**
   - All CRUD operations
   - Search functionality
   - User authentication (Identity still uses EF)
   - Image handling

---

## Key Architecture Decisions

✓ **Database Naming**: snake_case for all DB objects  
✓ **JSON Naming**: camelCase for API consistency  
✓ **Schema**: All app tables in `cooktime` schema, Identity in `public` schema  
✓ **Enum**: cooktime.unit type for measurement units  
✓ **Search**: pg_trgm + tsvector with GIN indexes  
✓ **Transactions**: Managed within stored procedures  
✓ **Connection**: Single NpgsqlDataSource singleton, reads from DATABASE_URL  
✓ **DTOs**: Use [JsonPropertyName] attributes for camelCase serialization  
✓ **Repositories**: Call stored procedures, deserialize JSON results  

---

## Next Steps

**Immediate**: Update controllers to use repositories  
**Short-term**: Remove EF dependencies, test migration  
**Long-term**: Implement data import script, full system testing
