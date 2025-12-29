using System.Text.Json;
using BabeAlgorithms.Models.Contracts;
using Npgsql;

namespace BabeAlgorithms.Services.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ReviewRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<List<ReviewDto>> GetByRecipeIdAsync(int recipeId)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipe_reviews($1)", conn);

        cmd.Parameters.AddWithValue(recipeId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return new List<ReviewDto>();

        var json = result.ToString()!;
        return JsonSerializer.Deserialize<List<ReviewDto>>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? new List<ReviewDto>();
    }
}
