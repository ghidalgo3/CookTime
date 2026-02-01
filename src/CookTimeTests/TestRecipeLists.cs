using BabeAlgorithms.Models.Contracts;
using Npgsql;

namespace CookTime.Test;

[TestClass]
public class TestRecipeLists : TestBase
{
    [TestInitialize]
    public async Task TestInitialize()
    {
        await InitializeAsync(nameof(TestRecipeLists));
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        await CleanupAsync();
    }

    [TestMethod]
    public async Task CreateAsync_ReturnsNewListId()
    {
        var createDto = new RecipeListCreateDto
        {
            OwnerId = TestUserId,
            Name = "Test List"
        };

        var listId = await Db.CreateRecipeListAsync(createDto);

        Assert.AreNotEqual(Guid.Empty, listId, "Expected a valid list ID");
    }

    [TestMethod]
    public async Task GetByUserIdAsync_ReturnsEmptyList_WhenNoLists()
    {
        var emptyUserId = await CreateTestUserAsync("Empty");

        try
        {
            var lists = await Db.GetRecipeListsAsync(emptyUserId);

            Assert.IsNotNull(lists);
            Assert.IsEmpty(lists);
        }
        finally
        {
            await DeleteTestUserAsync(emptyUserId);
        }
    }

    [TestMethod]
    public async Task GetByUserIdAsync_ReturnsLists_WhenListsExist()
    {
        var createDto = new RecipeListCreateDto
        {
            OwnerId = TestUserId,
            Name = "My Recipe List"
        };
        await Db.CreateRecipeListAsync(createDto);

        var lists = await Db.GetRecipeListsAsync(TestUserId);

        Assert.IsNotNull(lists);
        Assert.IsGreaterThanOrEqualTo(1, lists.Count);
        Assert.IsTrue(lists.Any(l => l.Name == "My Recipe List"));
    }

    [TestMethod]
    public async Task GetWithRecipesAsync_ReturnsNull_WhenListDoesNotExist()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await Db.GetRecipeListWithRecipesAsync(nonExistentId);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetWithRecipesAsync_ReturnsListWithEmptyRecipes_WhenNoRecipesAdded()
    {
        var createDto = new RecipeListCreateDto
        {
            OwnerId = TestUserId,
            Name = "Empty List"
        };
        var listId = await Db.CreateRecipeListAsync(createDto);

        var result = await Db.GetRecipeListWithRecipesAsync(listId);

        Assert.IsNotNull(result);
        Assert.AreEqual(listId, result.Id);
        Assert.AreEqual("Empty List", result.Name);
        Assert.IsNotNull(result.Recipes);
        Assert.IsEmpty(result.Recipes);
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
            OwnerId = TestUserId,
            Name = "List With Recipe"
        };
        var listId = await Db.CreateRecipeListAsync(createDto);

        await Db.AddRecipeToListAsync(listId, recipeId.Value, 2.0);

        var result = await Db.GetRecipeListWithRecipesAsync(listId);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Recipes.Any(r => r.Recipe.Id == recipeId.Value));
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
            OwnerId = TestUserId,
            Name = "List For Removal Test"
        };
        var listId = await Db.CreateRecipeListAsync(createDto);
        await Db.AddRecipeToListAsync(listId, recipeId.Value, 1.0);

        await Db.RemoveRecipeFromListAsync(listId, recipeId.Value);

        var result = await Db.GetRecipeListWithRecipesAsync(listId);
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Recipes.Any(r => r.Recipe.Id == recipeId.Value));
    }

    private async Task<Guid?> GetFirstRecipeId()
    {
        await using var conn = await DataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id FROM cooktime.recipes LIMIT 1", conn);
        var result = await cmd.ExecuteScalarAsync();
        return result as Guid?;
    }
}
