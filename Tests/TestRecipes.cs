using BabeAlgorithms.Models.Contracts;
using BabeAlgorithms.Services;
using Npgsql;

[TestClass]
public class TestRecipes
{
    private const string TEST_INGREDIENT_NAME = "Test Ingredient";
    private static NpgsqlDataSource _dataSource = null!;
    private static CookTimeDB _db = null!;
    private static Guid _testUserId;
    private static Guid _testIngredientId;

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
        await using var userCmd = new NpgsqlCommand(
            "INSERT INTO cooktime.users (id, provider, provider_user_id, email, display_name) VALUES ($1, $2, $3, $4, $5)", conn);
        userCmd.Parameters.AddWithValue(_testUserId);
        userCmd.Parameters.AddWithValue("test");
        userCmd.Parameters.AddWithValue(_testUserId.ToString());
        userCmd.Parameters.AddWithValue($"test-{_testUserId}@test.com");
        userCmd.Parameters.AddWithValue("Test User");
        await userCmd.ExecuteNonQueryAsync();

        // Create a test ingredient
        _testIngredientId = Guid.NewGuid();
        await using var ingredientCmd = new NpgsqlCommand(
            "INSERT INTO cooktime.ingredients (id, name) VALUES ($1, $2)", conn);
        ingredientCmd.Parameters.AddWithValue(_testIngredientId);
        ingredientCmd.Parameters.AddWithValue(TEST_INGREDIENT_NAME);
        await ingredientCmd.ExecuteNonQueryAsync();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        await using var conn = await _dataSource.OpenConnectionAsync();

        // Delete test recipes (cascade will handle components and requirements)
        await using var deleteRecipesCmd = new NpgsqlCommand(
            "DELETE FROM cooktime.recipes WHERE owner_id = $1", conn);
        deleteRecipesCmd.Parameters.AddWithValue(_testUserId);
        await deleteRecipesCmd.ExecuteNonQueryAsync();

        // Delete test ingredient
        await using var deleteIngredientCmd = new NpgsqlCommand(
            "DELETE FROM cooktime.ingredients WHERE id = $1", conn);
        deleteIngredientCmd.Parameters.AddWithValue(_testIngredientId);
        await deleteIngredientCmd.ExecuteNonQueryAsync();

        // Delete test user
        await using var deleteUserCmd = new NpgsqlCommand(
            "DELETE FROM cooktime.users WHERE id = $1", conn);
        deleteUserCmd.Parameters.AddWithValue(_testUserId);
        await deleteUserCmd.ExecuteNonQueryAsync();

        await _dataSource.DisposeAsync();
    }

    [TestMethod]
    public async Task CreateAsync_ReturnsNewRecipeId()
    {
        var createDto = new RecipeCreateDto
        {
            OwnerId = _testUserId,
            Name = "Test Recipe",
            Description = "A test recipe",
            Servings = 4,
            CookingMinutes = 30,
            PrepMinutes = 15,
            Components = new List<ComponentCreateDto>
            {
                new ComponentCreateDto
                {
                    Name = "Main",
                    Position = 1,
                    Steps = new List<string> { "Step 1", "Step 2" },
                    Ingredients = new List<IngredientRequirementCreateDto>
                    {
                        new IngredientRequirementCreateDto
                        {
                            IngredientId = _testIngredientId,
                            Quantity = 1.0,
                            Unit = "cup",
                            Position = 1
                        }
                    }
                }
            }
        };

        var recipeId = await _db.CreateRecipeAsync(createDto);

        Assert.AreNotEqual(Guid.Empty, recipeId, "Expected a valid recipe ID");
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsNull_WhenRecipeDoesNotExist()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _db.GetRecipeByIdAsync(nonExistentId);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsRecipe_WhenRecipeExists()
    {
        // Create a recipe first
        var createDto = new RecipeCreateDto
        {
            OwnerId = _testUserId,
            Name = "Recipe for GetById Test",
            Description = "Testing GetByIdAsync",
            Servings = 2,
            Components = new List<ComponentCreateDto>()
        };
        var recipeId = await _db.CreateRecipeAsync(createDto);

        var result = await _db.GetRecipeByIdAsync(recipeId);

        Assert.IsNotNull(result);
        Assert.AreEqual(recipeId, result.Id);
        Assert.AreEqual("Recipe for GetById Test", result.Name);
        Assert.AreEqual("Testing GetByIdAsync", result.Description);
        Assert.AreEqual(2, result.ServingsProduced);
    }

    [TestMethod]
    public async Task SearchByNameAsync_ReturnsEmptyList_WhenNoMatch()
    {
        var result = await _db.SearchRecipesAsync("xyznonexistentrecipename123");

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task SearchByNameAsync_ReturnsMatches_WhenRecipesExist()
    {
        // Create recipes with searchable names
        var createDto1 = new RecipeCreateDto
        {
            OwnerId = _testUserId,
            Name = "Chocolate Cake Supreme",
            Components = new List<ComponentCreateDto>()
        };
        var createDto2 = new RecipeCreateDto
        {
            OwnerId = _testUserId,
            Name = "Chocolate Chip Cookies",
            Components = new List<ComponentCreateDto>()
        };
        await _db.CreateRecipeAsync(createDto1);
        await _db.CreateRecipeAsync(createDto2);

        var result = await _db.SearchRecipesAsync("chocolate");

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count >= 2, "Expected at least 2 chocolate recipes");
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsRecipes()
    {
        // Create at least one recipe
        var createDto = new RecipeCreateDto
        {
            OwnerId = _testUserId,
            Name = "Recipe for GetAll Test",
            Components = new List<ComponentCreateDto>()
        };
        await _db.CreateRecipeAsync(createDto);

        var result = await _db.GetRecipesAsync(pageSize: 10, pageNumber: 1);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count >= 1);
    }

    [TestMethod]
    public async Task DeleteAsync_RemovesRecipe()
    {
        // Create a recipe to delete
        var createDto = new RecipeCreateDto
        {
            OwnerId = _testUserId,
            Name = "Recipe to Delete",
            Components = new List<ComponentCreateDto>()
        };
        var recipeId = await _db.CreateRecipeAsync(createDto);

        // Verify it exists
        var before = await _db.GetRecipeByIdAsync(recipeId);
        Assert.IsNotNull(before);

        // Delete it
        await _db.DeleteRecipeAsync(recipeId);

        // Verify it's gone
        var after = await _db.GetRecipeByIdAsync(recipeId);
        Assert.IsNull(after);
    }

    [TestMethod]
    public async Task UpdateAsync_UpdatesRecipe()
    {
        // Create a recipe to update
        var createDto = new RecipeCreateDto
        {
            OwnerId = _testUserId,
            Name = "Recipe Before Update",
            Description = "Original description",
            Servings = 2,
            Components = new List<ComponentCreateDto>()
        };
        var recipeId = await _db.CreateRecipeAsync(createDto);

        // Update it
        var updateDto = new RecipeUpdateDto
        {
            Id = recipeId,
            OwnerId = _testUserId,
            Name = "Recipe After Update",
            Description = "Updated description",
            Servings = 8,
            Components = new List<ComponentCreateDto>()
        };
        await _db.UpdateRecipeAsync(updateDto);

        // Verify the update
        var result = await _db.GetRecipeByIdAsync(recipeId);
        Assert.IsNotNull(result);
        Assert.AreEqual("Recipe After Update", result.Name);
        Assert.AreEqual("Updated description", result.Description);
        Assert.AreEqual(8, result.ServingsProduced);
    }

    [TestMethod]
    public async Task SearchByIngredientAsync_ReturnsRecipes_WithIngredient()
    {
        // Create a recipe with our test ingredient
        var createDto = new RecipeCreateDto
        {
            OwnerId = _testUserId,
            Name = "Recipe With Test Ingredient",
            Components = new List<ComponentCreateDto>
            {
                new ComponentCreateDto
                {
                    Name = "Main",
                    Position = 1,
                    Steps = new List<string>(),
                    Ingredients = new List<IngredientRequirementCreateDto>
                    {
                        new IngredientRequirementCreateDto
                        {
                            IngredientId = _testIngredientId,
                            Quantity = 2.0,
                            Unit = "tablespoon",
                            Position = 1
                        }
                    }
                }
            }
        };
        await _db.CreateRecipeAsync(createDto);

        var result = await _db.SearchRecipesAsync(TEST_INGREDIENT_NAME);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count >= 1);
        Assert.IsTrue(result.Any(r => r.Name == "Recipe With Test Ingredient"));
    }
}
