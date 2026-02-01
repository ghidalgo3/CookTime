using BabeAlgorithms.Services;
using Npgsql;

namespace CookTime.Test;

/// <summary>
/// Base class for integration tests that require database access.
/// Provides common setup/teardown for test users and database connections.
/// </summary>
public abstract class TestBase
{
    private static readonly string ConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
        ?? "Host=localhost;Database=cooktime;Username=cooktime;Password=development;Include Error Detail=true";

    protected NpgsqlDataSource DataSource { get; private set; } = null!;
    protected CookTimeDB Db { get; private set; } = null!;
    protected Guid TestUserId { get; private set; }

    protected async Task InitializeAsync(string testClassName)
    {
        DataSource = NpgsqlDataSource.Create(ConnectionString);
        Db = new CookTimeDB(DataSource);

        TestUserId = Guid.NewGuid();
        await using var conn = await DataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO cooktime.users (id, provider, provider_user_id, email, display_name) VALUES ($1, $2, $3, $4, $5)", conn);
        cmd.Parameters.AddWithValue(TestUserId);
        cmd.Parameters.AddWithValue("test");
        cmd.Parameters.AddWithValue(TestUserId.ToString());
        cmd.Parameters.AddWithValue($"test-{TestUserId}@test.com");
        cmd.Parameters.AddWithValue($"Test User {testClassName} {TestUserId}");
        await cmd.ExecuteNonQueryAsync();
    }

    protected async Task CleanupAsync()
    {
        await using var conn = await DataSource.OpenConnectionAsync();

        // Delete test recipes (cascade handles components and requirements)
        await using var deleteRecipesCmd = new NpgsqlCommand(
            "DELETE FROM cooktime.recipes WHERE owner_id = $1", conn);
        deleteRecipesCmd.Parameters.AddWithValue(TestUserId);
        await deleteRecipesCmd.ExecuteNonQueryAsync();

        // Delete test recipe lists (cascade handles list items)
        await using var deleteListsCmd = new NpgsqlCommand(
            "DELETE FROM cooktime.recipe_lists WHERE owner_id = $1", conn);
        deleteListsCmd.Parameters.AddWithValue(TestUserId);
        await deleteListsCmd.ExecuteNonQueryAsync();

        // Delete test user
        await using var deleteUserCmd = new NpgsqlCommand(
            "DELETE FROM cooktime.users WHERE id = $1", conn);
        deleteUserCmd.Parameters.AddWithValue(TestUserId);
        await deleteUserCmd.ExecuteNonQueryAsync();

        await DataSource.DisposeAsync();
    }

    /// <summary>
    /// Creates a temporary test user and returns its ID. 
    /// The caller is responsible for cleanup via DeleteTestUserAsync.
    /// </summary>
    protected async Task<Guid> CreateTestUserAsync(string nameSuffix = "")
    {
        var userId = Guid.NewGuid();
        await using var conn = await DataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO cooktime.users (id, provider, provider_user_id, email, display_name) VALUES ($1, $2, $3, $4, $5)", conn);
        cmd.Parameters.AddWithValue(userId);
        cmd.Parameters.AddWithValue("test");
        cmd.Parameters.AddWithValue(userId.ToString());
        cmd.Parameters.AddWithValue($"test-{userId}@test.com");
        cmd.Parameters.AddWithValue($"Test User {nameSuffix} {userId}");
        await cmd.ExecuteNonQueryAsync();
        return userId;
    }

    /// <summary>
    /// Deletes a test user created via CreateTestUserAsync.
    /// </summary>
    protected async Task DeleteTestUserAsync(Guid userId)
    {
        await using var conn = await DataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "DELETE FROM cooktime.users WHERE id = $1", conn);
        cmd.Parameters.AddWithValue(userId);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Creates a test ingredient and returns its ID.
    /// Optionally sets count_regex on the linked nutrition_facts.
    /// </summary>
    protected async Task<Guid> CreateTestIngredientAsync(string name, Guid? nutritionFactsId = null, string? countRegex = null)
    {
        var ingredientId = Guid.NewGuid();
        await using var conn = await DataSource.OpenConnectionAsync();

        // Update count_regex on nutrition_facts if provided
        if (nutritionFactsId.HasValue && !string.IsNullOrEmpty(countRegex))
        {
            await using var updateCmd = new NpgsqlCommand(
                "UPDATE cooktime.nutrition_facts SET count_regex = $1 WHERE id = $2", conn);
            updateCmd.Parameters.AddWithValue(countRegex);
            updateCmd.Parameters.AddWithValue(nutritionFactsId.Value);
            await updateCmd.ExecuteNonQueryAsync();
        }

        await using var cmd = new NpgsqlCommand(
            "INSERT INTO cooktime.ingredients (id, name, nutrition_facts_id) VALUES ($1, $2, $3)", conn);
        cmd.Parameters.AddWithValue(ingredientId);
        cmd.Parameters.AddWithValue(name);
        cmd.Parameters.AddWithValue(nutritionFactsId.HasValue ? nutritionFactsId.Value : DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
        return ingredientId;
    }

    /// <summary>
    /// Deletes a test ingredient and any ingredient_requirements that reference it.
    /// </summary>
    protected async Task DeleteTestIngredientAsync(Guid ingredientId)
    {
        await using var conn = await DataSource.OpenConnectionAsync();

        // First delete any ingredient_requirements that reference this ingredient
        await using var deleteReqsCmd = new NpgsqlCommand(
            "DELETE FROM cooktime.ingredient_requirements WHERE ingredient_id = $1", conn);
        deleteReqsCmd.Parameters.AddWithValue(ingredientId);
        await deleteReqsCmd.ExecuteNonQueryAsync();

        // Now delete the ingredient itself
        await using var cmd = new NpgsqlCommand(
            "DELETE FROM cooktime.ingredients WHERE id = $1", conn);
        cmd.Parameters.AddWithValue(ingredientId);
        await cmd.ExecuteNonQueryAsync();
    }
}
