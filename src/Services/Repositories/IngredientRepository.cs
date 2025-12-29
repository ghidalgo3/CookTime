using System.Text.Json;
using BabeAlgorithms.Models.Contracts;
using Npgsql;

namespace BabeAlgorithms.Services.Repositories;

public class IngredientRepository : IIngredientRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public IngredientRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<int> CreateAsync(IngredientCreateDto ingredient)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.create_ingredient($1)", conn);

        cmd.Parameters.AddWithValue(JsonSerializer.Serialize(ingredient, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        var result = await cmd.ExecuteScalarAsync();
        return (int)result!;
    }

    public async Task<IngredientDto?> GetByIdAsync(int ingredientId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_ingredient($1)", conn);

        cmd.Parameters.AddWithValue(ingredientId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return null;

        var json = result.ToString()!;
        return JsonSerializer.Deserialize<IngredientDto>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public async Task<List<IngredientDto>> SearchAsync(string searchTerm, int limit = 20)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.search_ingredients($1, $2)", conn);

        cmd.Parameters.AddWithValue(searchTerm);
        cmd.Parameters.AddWithValue(limit);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return new List<IngredientDto>();

        var json = result.ToString()!;
        return JsonSerializer.Deserialize<List<IngredientDto>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? new List<IngredientDto>();
    }

    public async Task<List<string>> GetImagesAsync(int ingredientId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_ingredient_images($1)", conn);

        cmd.Parameters.AddWithValue(ingredientId);

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
