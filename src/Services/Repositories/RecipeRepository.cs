using System.Text.Json;
using BabeAlgorithms.Models.Contracts;
using Npgsql;
using NpgsqlTypes;

namespace BabeAlgorithms.Services.Repositories;

public class RecipeRepository : IRecipeRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public RecipeRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Guid> CreateAsync(RecipeCreateDto recipe)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.create_recipe($1::jsonb)", conn);

        cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, JsonSerializer.Serialize(recipe, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        var result = await cmd.ExecuteScalarAsync();
        return (Guid)result!;
    }

    public async Task UpdateAsync(RecipeUpdateDto recipe)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.update_recipe($1, $2::jsonb)", conn);

        cmd.Parameters.AddWithValue(recipe.Id);
        cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, JsonSerializer.Serialize(recipe, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid recipeId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.delete_recipe($1)", conn);

        cmd.Parameters.AddWithValue(recipeId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<RecipeDetailDto?> GetByIdAsync(Guid recipeId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipe_with_details($1)", conn);

        cmd.Parameters.AddWithValue(recipeId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return null;

        var json = result.ToString()!;
        return JsonSerializer.Deserialize<RecipeDetailDto>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public async Task<List<RecipeSummaryDto>> SearchByNameAsync(string searchTerm)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.search_recipes_by_name($1)", conn);

        cmd.Parameters.AddWithValue(searchTerm);

        var results = new List<RecipeSummaryDto>();
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var json = reader.GetString(0);
            var dto = JsonSerializer.Deserialize<RecipeSummaryDto>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            if (dto != null)
                results.Add(dto);
        }

        return results;
    }

    public async Task<List<RecipeSummaryDto>> SearchByIngredientAsync(Guid ingredientId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.search_recipes_by_ingredient($1)", conn);

        cmd.Parameters.AddWithValue(ingredientId);

        var results = new List<RecipeSummaryDto>();
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var json = reader.GetString(0);
            var dto = JsonSerializer.Deserialize<RecipeSummaryDto>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            if (dto != null)
                results.Add(dto);
        }

        return results;
    }

    public async Task<List<RecipeSummaryDto>> GetAllAsync(int pageSize = 50, int pageNumber = 1)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipes($1, $2)", conn);

        cmd.Parameters.AddWithValue(pageSize);
        cmd.Parameters.AddWithValue(pageNumber);

        var results = new List<RecipeSummaryDto>();
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var json = reader.GetString(0);
            var dto = JsonSerializer.Deserialize<RecipeSummaryDto>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            if (dto != null)
                results.Add(dto);
        }

        return results;
    }

    public async Task<List<string>> GetImagesAsync(Guid recipeId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipe_images($1)", conn);

        cmd.Parameters.AddWithValue(recipeId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return new List<string>();

        var json = result.ToString()!;
        return JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? new List<string>();
    }
}
