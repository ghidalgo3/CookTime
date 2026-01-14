using BabeAlgorithms.Models.Contracts;
using BabeAlgorithms.Services;
using Npgsql;

[TestClass]
public class TestRecipeLists
{
    private static NpgsqlDataSource _dataSource = null!;
    private static CookTimeDB _db = null!;
    private static Guid _testUserId;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Database=cooktime;Username=cooktime;Password=development;Include Error Detail=true";

        _dataSource = NpgsqlDataSource.Create(connectionString);
        _db = new CookTimeDB(_dataSource);

        // Create a test user
        _testUserId = Guid.NewGuid();
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO cooktime.users (id, provider, provider_user_id, email, display_name) VALUES ($1, $2, $3, $4, $5)", conn);
        cmd.Parameters.AddWithValue(_testUserId);
        cmd.Parameters.AddWithValue("test");
        cmd.Parameters.AddWithValue(_testUserId.ToString());
        cmd.Parameters.AddWithValue($"test-{_testUserId}@test.com");
        cmd.Parameters.AddWithValue("Test User");
        await cmd.ExecuteNonQueryAsync();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        // Clean up test data - recipe_lists cascade deletes recipe_requirements
        await using var conn = await _dataSource.OpenConnectionAsync();

        await using var deleteListsCmd = new NpgsqlCommand(
            "DELETE FROM cooktime.recipe_lists WHERE owner_id = $1", conn);
        deleteListsCmd.Parameters.AddWithValue(_testUserId);
        await deleteListsCmd.ExecuteNonQueryAsync();

        await using var deleteUserCmd = new NpgsqlCommand(
            "DELETE FROM cooktime.users WHERE id = $1", conn);
        deleteUserCmd.Parameters.AddWithValue(_testUserId);
        await deleteUserCmd.ExecuteNonQueryAsync();

        await _dataSource.DisposeAsync();
    }

    [TestMethod]
    public async Task CreateAsync_ReturnsNewListId()
    {
        var createDto = new RecipeListCreateDto
        {
            OwnerId = _testUserId,
            Name = "Test List"
        };

        var listId = await _db.CreateRecipeListAsync(createDto);

        Assert.AreNotEqual(Guid.Empty, listId, "Expected a valid list ID");
    }

    [TestMethod]
    public async Task GetByUserIdAsync_ReturnsEmptyList_WhenNoLists()
    {
        // Create a user with no lists
        var emptyUserId = Guid.NewGuid();
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO cooktime.users (id, provider, provider_user_id, email, display_name) VALUES ($1, $2, $3, $4, $5)", conn);
        cmd.Parameters.AddWithValue(emptyUserId);
        cmd.Parameters.AddWithValue("test");
        cmd.Parameters.AddWithValue(emptyUserId.ToString());
        cmd.Parameters.AddWithValue($"empty-{emptyUserId}@test.com");
        cmd.Parameters.AddWithValue("Empty User");
        await cmd.ExecuteNonQueryAsync();

        try
        {
            var lists = await _db.GetRecipeListsByUserIdAsync(emptyUserId);

            Assert.IsNotNull(lists);
            Assert.AreEqual(0, lists.Count);
        }
        finally
        {
            await using var deleteCmd = new NpgsqlCommand(
                "DELETE FROM cooktime.users WHERE id = $1", conn);
            deleteCmd.Parameters.AddWithValue(emptyUserId);
            await deleteCmd.ExecuteNonQueryAsync();
        }
    }

    [TestMethod]
    public async Task GetByUserIdAsync_ReturnsLists_WhenListsExist()
    {
        var createDto = new RecipeListCreateDto
        {
            OwnerId = _testUserId,
            Name = "My Recipe List"
        };
        await _db.CreateRecipeListAsync(createDto);

        var lists = await _db.GetRecipeListsByUserIdAsync(_testUserId);

        Assert.IsNotNull(lists);
        Assert.IsTrue(lists.Count >= 1);
        Assert.IsTrue(lists.Any(l => l.Name == "My Recipe List"));
    }

    [TestMethod]
    public async Task GetWithRecipesAsync_ReturnsNull_WhenListDoesNotExist()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _db.GetRecipeListWithRecipesAsync(nonExistentId);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetWithRecipesAsync_ReturnsListWithEmptyRecipes_WhenNoRecipesAdded()
    {
        var createDto = new RecipeListCreateDto
        {
            OwnerId = _testUserId,
            Name = "Empty List"
        };
        var listId = await _db.CreateRecipeListAsync(createDto);

        var result = await _db.GetRecipeListWithRecipesAsync(listId);

        Assert.IsNotNull(result);
        Assert.AreEqual(listId, result.Id);
        Assert.AreEqual("Empty List", result.Name);
        Assert.IsNotNull(result.Recipes);
        Assert.AreEqual(0, result.Recipes.Count);
    }

    [TestMethod]
    public async Task AddRecipeAsync_AddsRecipeToList()
    {
        // First, get an existing recipe ID from the database
        var recipeId = await GetFirstRecipeId();
        if (recipeId == null)
        {
            Assert.Inconclusive("No recipes in database to test with");
            return;
        }

        var createDto = new RecipeListCreateDto
        {
            OwnerId = _testUserId,
            Name = "List With Recipe"
        };
        var listId = await _db.CreateRecipeListAsync(createDto);

        await _db.AddRecipeToListAsync(listId, recipeId.Value, 2.0);

        var result = await _db.GetRecipeListWithRecipesAsync(listId);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Recipes.Any(r => r.RecipeId == recipeId.Value));
    }

    [TestMethod]
    public async Task RemoveRecipeAsync_RemovesRecipeFromList()
    {
        var recipeId = await GetFirstRecipeId();
        if (recipeId == null)
        {
            Assert.Inconclusive("No recipes in database to test with");
            return;
        }

        var createDto = new RecipeListCreateDto
        {
            OwnerId = _testUserId,
            Name = "List For Removal Test"
        };
        var listId = await _db.CreateRecipeListAsync(createDto);
        await _db.AddRecipeToListAsync(listId, recipeId.Value, 1.0);

        await _db.RemoveRecipeFromListAsync(listId, recipeId.Value);

        var result = await _db.GetRecipeListWithRecipesAsync(listId);
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Recipes.Any(r => r.RecipeId == recipeId.Value));
    }

    private static async Task<Guid?> GetFirstRecipeId()
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id FROM cooktime.recipes LIMIT 1", conn);
        var result = await cmd.ExecuteScalarAsync();
        return result as Guid?;
    }
}
