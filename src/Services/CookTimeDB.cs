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

    public async Task<List<RecipeSummaryDto>> SearchRecipesByNameAsync(string searchTerm)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.search_recipes_by_name($1)", conn);

        cmd.Parameters.AddWithValue(searchTerm);

        return await ReadRecipeSummaryListAsync(cmd);
    }

    public async Task<List<RecipeSummaryDto>> SearchRecipesByIngredientAsync(Guid ingredientId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.search_recipes_by_ingredient($1)", conn);

        cmd.Parameters.AddWithValue(ingredientId);

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

    public async Task<List<RecipeListDto>> GetRecipeListsByUserIdAsync(Guid userId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_user_recipe_lists($1)", conn);

        cmd.Parameters.AddWithValue(userId);

        var results = new List<RecipeListDto>();
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var json = reader.GetString(0);
            var dto = JsonSerializer.Deserialize<RecipeListDto>(json, JsonOptions);
            if (dto != null)
                results.Add(dto);
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

    public async Task<List<IngredientDto>> SearchIngredientsAsync(string searchTerm, int limit = 20)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.search_ingredients($1, $2)", conn);

        cmd.Parameters.AddWithValue(searchTerm);
        cmd.Parameters.AddWithValue(limit);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return [];

        return JsonSerializer.Deserialize<List<IngredientDto>>(result.ToString()!, JsonOptions) ?? [];
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

    #endregion

    #region Reviews

    public async Task<List<ReviewDto>> GetReviewsByRecipeIdAsync(int recipeId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipe_reviews($1)", conn);

        cmd.Parameters.AddWithValue(recipeId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return [];

        return JsonSerializer.Deserialize<List<ReviewDto>>(result.ToString()!, JsonOptions) ?? [];
    }

    #endregion
}
