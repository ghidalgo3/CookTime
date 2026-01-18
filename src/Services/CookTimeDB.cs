using BabeAlgorithms.Models.Contracts;
using Npgsql;
using NpgsqlTypes;

namespace BabeAlgorithms.Services;

public class CookTimeDB(NpgsqlDataSource dataSource)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    #region Recipes

    public async Task<Guid> CreateRecipeAsync(RecipeCreateDto recipe)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.create_recipe($1::jsonb)", conn);

        cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, JsonSerializer.Serialize(recipe, JsonOptions));

        var result = await cmd.ExecuteScalarAsync();
        return (Guid)result!;
    }

    public async Task UpdateRecipeAsync(RecipeUpdateDto recipe)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.update_recipe($1, $2::jsonb)", conn);

        cmd.Parameters.AddWithValue(recipe.Id);
        cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, JsonSerializer.Serialize(recipe, JsonOptions));

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteRecipeAsync(Guid recipeId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.delete_recipe($1)", conn);

        cmd.Parameters.AddWithValue(recipeId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<RecipeDetailDto?> GetRecipeByIdAsync(Guid recipeId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipe_with_details($1)", conn);

        cmd.Parameters.AddWithValue(recipeId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return null;

        return JsonSerializer.Deserialize<RecipeDetailDto>(result.ToString()!, JsonOptions);
    }

    public async Task<List<RecipeSummaryDto>> SearchRecipesAsync(string searchTerm, int pageSize = 50, int pageNumber = 1)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.search_recipes($1, $2, $3)", conn);

        cmd.Parameters.AddWithValue(searchTerm);
        cmd.Parameters.AddWithValue(pageSize);
        cmd.Parameters.AddWithValue(pageNumber);

        return await ReadRecipeSummaryListAsync(cmd);
    }

    public async Task<List<RecipeSummaryDto>> GetRecipesAsync(int pageSize = 50, int pageNumber = 1)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipes($1, $2)", conn);

        cmd.Parameters.AddWithValue(pageSize);
        cmd.Parameters.AddWithValue(pageNumber);

        return await ReadRecipeSummaryListAsync(cmd);
    }

    public async Task<List<string>> GetRecipeImagesAsync(Guid recipeId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipe_images($1)", conn);

        cmd.Parameters.AddWithValue(recipeId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(result.ToString()!, JsonOptions) ?? [];
    }

    public async Task<long> GetRecipeCountAsync()
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM cooktime.recipes", conn);

        var result = await cmd.ExecuteScalarAsync();
        return (long)result!;
    }

    public async Task<List<RecipeSummaryDto>> GetNewRecipesAsync(int count = 3)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(@"
            SELECT cooktime.recipe_to_summary(r)::text
            FROM cooktime.recipes r
            ORDER BY r.last_modified_date DESC
            LIMIT $1", conn);

        cmd.Parameters.AddWithValue(count);

        return await ReadRecipeSummaryListAsync(cmd);
    }

    public async Task<List<RecipeSummaryDto>> GetFeaturedRecipesAsync()
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        // WHERE EXISTS (SELECT 1 FROM cooktime.images i WHERE i.recipe_id = r.id)
        await using var cmd = new NpgsqlCommand(@"
            SELECT cooktime.recipe_to_summary(r)::text
            FROM cooktime.recipes r
            ORDER BY RANDOM()
            LIMIT 3", conn);

        return await ReadRecipeSummaryListAsync(cmd);
    }

    public async Task<List<RecipeSummaryDto>> GetRecipesByUserAsync(Guid userId, int pageSize = 50, int pageNumber = 1)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipes_by_user($1, $2, $3)", conn);

        cmd.Parameters.AddWithValue(userId);
        cmd.Parameters.AddWithValue(pageSize);
        cmd.Parameters.AddWithValue(pageNumber);

        return await ReadRecipeSummaryListAsync(cmd);
    }


    private static async Task<List<RecipeSummaryDto>> ReadRecipeSummaryListAsync(NpgsqlCommand cmd)
    {
        var results = new List<RecipeSummaryDto>();
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var json = reader.GetString(0);
            var dto = JsonSerializer.Deserialize<RecipeSummaryDto>(json, JsonOptions);
            if (dto != null)
            {
                results.Add(dto);
            }
        }

        return results;
    }

    #endregion

    #region Recipe Lists

    public async Task<Guid> CreateRecipeListAsync(RecipeListCreateDto recipeList)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.create_recipe_list($1::jsonb)", conn);

        cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, JsonSerializer.Serialize(recipeList, JsonOptions));

        var result = await cmd.ExecuteScalarAsync();
        return (Guid)result!;
    }

    public async Task<List<RecipeListDto>> GetRecipeListsAsync(Guid userId, string? filter = null)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_user_recipe_lists($1, $2)", conn);

        cmd.Parameters.AddWithValue(userId);
        cmd.Parameters.AddWithValue(filter ?? (object)DBNull.Value);

        var results = new List<RecipeListDto>();
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var json = reader.GetString(0);
            var dto = JsonSerializer.Deserialize<RecipeListDto>(json, JsonOptions);
            if (dto != null)
            {
                results.Add(dto);
            }
        }

        return results;
    }

    public async Task<RecipeListWithRecipesDto?> GetRecipeListWithRecipesAsync(Guid listId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipe_list_with_recipes($1)", conn);

        cmd.Parameters.AddWithValue(listId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return null;

        return JsonSerializer.Deserialize<RecipeListWithRecipesDto>(result.ToString()!, JsonOptions);
    }

    public async Task AddRecipeToListAsync(Guid listId, Guid recipeId, double quantity)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.add_recipe_to_list($1, $2, $3)", conn);

        cmd.Parameters.AddWithValue(listId);
        cmd.Parameters.AddWithValue(recipeId);
        cmd.Parameters.AddWithValue(quantity);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task RemoveRecipeFromListAsync(Guid listId, Guid recipeId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM cooktime.recipe_requirements WHERE recipe_list_id = $1 AND recipe_id = $2", conn);

        cmd.Parameters.AddWithValue(listId);
        cmd.Parameters.AddWithValue(recipeId);

        await cmd.ExecuteNonQueryAsync();
    }

    #endregion

    #region Ingredients

    public async Task<int> CreateIngredientAsync(IngredientCreateDto ingredient)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.create_ingredient($1)", conn);

        cmd.Parameters.AddWithValue(JsonSerializer.Serialize(ingredient, JsonOptions));

        var result = await cmd.ExecuteScalarAsync();
        return (int)result!;
    }

    public async Task<IngredientDto?> GetIngredientByIdAsync(int ingredientId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_ingredient($1)", conn);

        cmd.Parameters.AddWithValue(ingredientId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return null;

        return JsonSerializer.Deserialize<IngredientDto>(result.ToString()!, JsonOptions);
    }

    public async Task<List<IngredientAutosuggestDto>> SearchIngredientsAsync(string searchTerm)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.search_ingredients($1)", conn);

        cmd.Parameters.AddWithValue(searchTerm);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return [];

        return JsonSerializer.Deserialize<List<IngredientAutosuggestDto>>(result.ToString()!, JsonOptions) ?? [];
    }

    public async Task<List<string>> GetIngredientImagesAsync(int ingredientId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_ingredient_images($1)", conn);

        cmd.Parameters.AddWithValue(ingredientId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return [];

        return JsonSerializer.Deserialize<List<string>>(result.ToString()!, JsonOptions) ?? [];
    }

    public async Task<List<IngredientInternalUpdateDto>> GetIngredientsForAdminAsync()
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_ingredients_for_admin()", conn);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return [];

        return JsonSerializer.Deserialize<List<IngredientInternalUpdateDto>>(result.ToString()!, JsonOptions) ?? [];
    }

    public async Task<IngredientInternalUpdateDto?> UpdateIngredientInternalAsync(IngredientInternalUpdateDto update)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT cooktime.update_ingredient_internal($1, $2, $3, $4, $5, $6)", conn);

        cmd.Parameters.AddWithValue(update.IngredientId);
        cmd.Parameters.AddWithValue(update.IngredientNames);
        cmd.Parameters.AddWithValue(double.TryParse(update.ExpectedUnitMass, out var mass) ? mass : 0.1);
        cmd.Parameters.AddWithValue(update.NdbNumber);
        cmd.Parameters.AddWithValue(update.GtinUpc ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue(update.CountRegex ?? (object)DBNull.Value);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return null;

        return JsonSerializer.Deserialize<IngredientInternalUpdateDto>(result.ToString()!, JsonOptions);
    }

    #endregion

    #region Categories

    public async Task<int> CreateCategoryAsync(CategoryCreateDto category)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.create_category($1)", conn);

        cmd.Parameters.AddWithValue(JsonSerializer.Serialize(category, JsonOptions));

        var result = await cmd.ExecuteScalarAsync();
        return (int)result!;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_categories()", conn);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return [];

        return JsonSerializer.Deserialize<List<CategoryDto>>(result.ToString()!, JsonOptions) ?? [];
    }

    public async Task<List<CategoryWithIdDto>> SearchCategoriesAsync(string query)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.search_categories($1)", conn);
        cmd.Parameters.AddWithValue(query);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return [];

        return JsonSerializer.Deserialize<List<CategoryWithIdDto>>(result.ToString()!, JsonOptions) ?? [];
    }

    public async Task<List<CategoryWithIdDto>> GetAllCategoriesWithIdsAsync()
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_all_categories()", conn);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return [];

        return JsonSerializer.Deserialize<List<CategoryWithIdDto>>(result.ToString()!, JsonOptions) ?? [];
    }

    #endregion

    #region Reviews

    public async Task<List<ReviewViewDto>> GetReviewsByRecipeIdAsync(Guid recipeId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipe_reviews($1)", conn);

        cmd.Parameters.AddWithValue(recipeId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<ReviewViewDto>>(result.ToString()!, JsonOptions) ?? [];
    }

    public async Task<Guid> CreateReviewAsync(Guid recipeId, Guid ownerId, int rating, string? text)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.create_review($1, $2, $3, $4)", conn);

        cmd.Parameters.AddWithValue(recipeId);
        cmd.Parameters.AddWithValue(ownerId);
        cmd.Parameters.AddWithValue(rating);
        cmd.Parameters.AddWithValue(text ?? (object)DBNull.Value);

        var result = await cmd.ExecuteScalarAsync();
        return (Guid)result!;
    }

    #endregion

    #region Images

    public async Task<List<ImageDto>> GetImagesByRecipeIdAsync(Guid recipeId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipe_images($1)", conn);

        cmd.Parameters.AddWithValue(recipeId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<ImageDto>>(result.ToString()!, JsonOptions) ?? [];
    }

    #endregion
}
