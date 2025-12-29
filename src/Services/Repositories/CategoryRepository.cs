using System.Text.Json;
using BabeAlgorithms.Models.Contracts;
using Npgsql;

namespace BabeAlgorithms.Services.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public CategoryRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<int> CreateAsync(CategoryCreateDto category)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.create_category($1)", conn);

        cmd.Parameters.AddWithValue(JsonSerializer.Serialize(category, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        var result = await cmd.ExecuteScalarAsync();
        return (int)result!;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_categories()", conn);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return new List<CategoryDto>();

        var json = result.ToString()!;
        return JsonSerializer.Deserialize<List<CategoryDto>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? new List<CategoryDto>();
    }
}
