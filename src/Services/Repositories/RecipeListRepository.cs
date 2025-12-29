using System.Text.Json;
using BabeAlgorithms.Models.Contracts;
using Npgsql;

namespace BabeAlgorithms.Services.Repositories;

public class RecipeListRepository : IRecipeListRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public RecipeListRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<int> CreateAsync(RecipeListCreateDto recipeList)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.create_recipe_list($1)", conn);

        cmd.Parameters.AddWithValue(JsonSerializer.Serialize(recipeList, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        var result = await cmd.ExecuteScalarAsync();
        return (int)result!;
    }

    public async Task<List<RecipeListDto>> GetByUserIdAsync(string userId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_user_recipe_lists($1)", conn);

        cmd.Parameters.AddWithValue(userId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return new List<RecipeListDto>();

        var json = result.ToString()!;
        return JsonSerializer.Deserialize<List<RecipeListDto>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? new List<RecipeListDto>();
    }

    public async Task<RecipeListWithRecipesDto?> GetWithRecipesAsync(int listId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipe_list_with_recipes($1)", conn);

        cmd.Parameters.AddWithValue(listId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return null;

        var json = result.ToString()!;
        return JsonSerializer.Deserialize<RecipeListWithRecipesDto>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public async Task AddRecipeAsync(int listId, int recipeId, int servings)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.add_recipe_to_list($1, $2, $3)", conn);

        cmd.Parameters.AddWithValue(listId);
        cmd.Parameters.AddWithValue(recipeId);
        cmd.Parameters.AddWithValue(servings);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task RemoveRecipeAsync(int listId, int recipeId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM cooktime.recipe_requirements WHERE list_id = $1 AND recipe_id = $2", conn);

        cmd.Parameters.AddWithValue(listId);
        cmd.Parameters.AddWithValue(recipeId);

        await cmd.ExecuteNonQueryAsync();
    }
}
