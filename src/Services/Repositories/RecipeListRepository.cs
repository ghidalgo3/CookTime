using BabeAlgorithms.Models.Contracts;
using Npgsql;
using NpgsqlTypes;

namespace BabeAlgorithms.Services.Repositories;

public class RecipeListRepository : IRecipeListRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public RecipeListRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Guid> CreateAsync(RecipeListCreateDto recipeList)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.create_recipe_list($1::jsonb)", conn);

        cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, JsonSerializer.Serialize(recipeList, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        var result = await cmd.ExecuteScalarAsync();
        return (Guid)result!;
    }

    public async Task<List<RecipeListDto>> GetByUserIdAsync(Guid userId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_user_recipe_lists($1)", conn);

        cmd.Parameters.AddWithValue(userId);

        var results = new List<RecipeListDto>();
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var json = reader.GetString(0);
            var dto = JsonSerializer.Deserialize<RecipeListDto>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            if (dto != null)
                results.Add(dto);
        }

        return results;
    }

    public async Task<RecipeListWithRecipesDto?> GetWithRecipesAsync(Guid listId)
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

    public async Task AddRecipeAsync(Guid listId, Guid recipeId, double quantity)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.add_recipe_to_list($1, $2, $3)", conn);

        cmd.Parameters.AddWithValue(listId);
        cmd.Parameters.AddWithValue(recipeId);
        cmd.Parameters.AddWithValue(quantity);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task RemoveRecipeAsync(Guid listId, Guid recipeId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM cooktime.recipe_requirements WHERE recipe_list_id = $1 AND recipe_id = $2", conn);

        cmd.Parameters.AddWithValue(listId);
        cmd.Parameters.AddWithValue(recipeId);

        await cmd.ExecuteNonQueryAsync();
    }
}
