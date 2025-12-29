using System.Text.Json;
using BabeAlgorithms.Models.Contracts;
using Npgsql;

namespace BabeAlgorithms.Services.Repositories;

public class RecipeRepository : IRecipeRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public RecipeRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<int> CreateAsync(RecipeCreateDto recipe)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.create_recipe($1)", conn);

        cmd.Parameters.AddWithValue(JsonSerializer.Serialize(recipe, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        var result = await cmd.ExecuteScalarAsync();
        return (int)result!;
    }

    public async Task UpdateAsync(RecipeUpdateDto recipe)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.update_recipe($1)", conn);

        cmd.Parameters.AddWithValue(JsonSerializer.Serialize(recipe, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int recipeId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.delete_recipe($1)", conn);

        cmd.Parameters.AddWithValue(recipeId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<RecipeDetailDto?> GetByIdAsync(int recipeId)
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

    public async Task<List<RecipeSummaryDto>> SearchByNameAsync(string searchTerm, int limit = 20, int offset = 0)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.search_recipes_by_name($1, $2, $3)", conn);

        cmd.Parameters.AddWithValue(searchTerm);
        cmd.Parameters.AddWithValue(limit);
        cmd.Parameters.AddWithValue(offset);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return new List<RecipeSummaryDto>();

        var json = result.ToString()!;
        return JsonSerializer.Deserialize<List<RecipeSummaryDto>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? new List<RecipeSummaryDto>();
    }

    public async Task<List<RecipeSummaryDto>> SearchByIngredientAsync(string ingredientName, int limit = 20, int offset = 0)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.search_recipes_by_ingredient($1, $2, $3)", conn);

        cmd.Parameters.AddWithValue(ingredientName);
        cmd.Parameters.AddWithValue(limit);
        cmd.Parameters.AddWithValue(offset);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return new List<RecipeSummaryDto>();

        var json = result.ToString()!;
        return JsonSerializer.Deserialize<List<RecipeSummaryDto>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? new List<RecipeSummaryDto>();
    }

    public async Task<List<RecipeSummaryDto>> GetAllAsync(int limit = 20, int offset = 0)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipes($1, $2)", conn);

        cmd.Parameters.AddWithValue(limit);
        cmd.Parameters.AddWithValue(offset);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return new List<RecipeSummaryDto>();

        var json = result.ToString()!;
        return JsonSerializer.Deserialize<List<RecipeSummaryDto>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? new List<RecipeSummaryDto>();
    }

    public async Task<List<string>> GetImagesAsync(int recipeId)
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
