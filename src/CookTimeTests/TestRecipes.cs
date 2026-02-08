using CookTime.Models.Contracts;

namespace CookTime.Test;

[TestClass]
public class TestRecipes : TestBase
{
    private const string TEST_INGREDIENT_NAME = "Test Ingredient";
    private Guid _testIngredientId;

    [TestInitialize]
    public async Task TestInitialize()
    {
        await InitializeAsync(nameof(TestRecipes));
        _testIngredientId = await CreateTestIngredientAsync(TEST_INGREDIENT_NAME);
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        await DeleteTestIngredientAsync(_testIngredientId);
        await CleanupAsync();
    }

    [TestMethod]
    public async Task CreateAsync_ReturnsNewRecipeId()
    {
        var createDto = new RecipeCreateDto
        {
            OwnerId = TestUserId,
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
                            Ingredient = new IngredientRefDto { Id = _testIngredientId, Name = TEST_INGREDIENT_NAME },
                            Quantity = 1.0,
                            Unit = "cup",
                            Position = 1
                        }
                    }
                }
            }
        };

        var recipeId = await Db.CreateRecipeAsync(createDto);

        Assert.AreNotEqual(Guid.Empty, recipeId, "Expected a valid recipe ID");
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsNull_WhenRecipeDoesNotExist()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await Db.GetRecipeByIdAsync(nonExistentId);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsRecipe_WhenRecipeExists()
    {
        // Create a recipe first
        var createDto = new RecipeCreateDto
        {
            OwnerId = TestUserId,
            Name = "Recipe for GetById Test",
            Description = "Testing GetByIdAsync",
            Servings = 2,
            Components = new List<ComponentCreateDto>()
        };
        var recipeId = await Db.CreateRecipeAsync(createDto);

        var result = await Db.GetRecipeByIdAsync(recipeId);

        Assert.IsNotNull(result);
        Assert.AreEqual(recipeId, result.Id);
        Assert.AreEqual("Recipe for GetById Test", result.Name);
        Assert.AreEqual("Testing GetByIdAsync", result.Description);
        Assert.AreEqual(2, result.ServingsProduced);
    }

    [TestMethod]
    public async Task SearchByNameAsync_ReturnsEmptyList_WhenNoMatch()
    {
        var result = await Db.SearchRecipesAsync("xyznonexistentrecipename123");

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task SearchByNameAsync_ReturnsMatches_WhenRecipesExist()
    {
        // Create recipes with searchable names
        var createDto1 = new RecipeCreateDto
        {
            OwnerId = TestUserId,
            Name = "Chocolate Cake Supreme",
            Components = new List<ComponentCreateDto>()
        };
        var createDto2 = new RecipeCreateDto
        {
            OwnerId = TestUserId,
            Name = "Chocolate Chip Cookies",
            Components = new List<ComponentCreateDto>()
        };
        await Db.CreateRecipeAsync(createDto1);
        await Db.CreateRecipeAsync(createDto2);

        var result = await Db.SearchRecipesAsync("chocolate");

        Assert.IsNotNull(result);
        Assert.IsGreaterThanOrEqualTo(2, result.Count, "Expected at least 2 chocolate recipes");
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsRecipes()
    {
        // Create at least one recipe
        var createDto = new RecipeCreateDto
        {
            OwnerId = TestUserId,
            Name = "Recipe for GetAll Test",
            Components = new List<ComponentCreateDto>()
        };
        await Db.CreateRecipeAsync(createDto);

        var result = await Db.GetRecipesAsync(pageSize: 10, pageNumber: 1);

        Assert.IsNotNull(result);
        Assert.IsGreaterThanOrEqualTo(1, result.Count);
    }

    [TestMethod]
    public async Task DeleteAsync_RemovesRecipe()
    {
        // Create a recipe to delete
        var createDto = new RecipeCreateDto
        {
            OwnerId = TestUserId,
            Name = "Recipe to Delete",
            Components = new List<ComponentCreateDto>()
        };
        var recipeId = await Db.CreateRecipeAsync(createDto);

        // Verify it exists
        var before = await Db.GetRecipeByIdAsync(recipeId);
        Assert.IsNotNull(before);

        // Delete it
        await Db.DeleteRecipeAsync(recipeId);

        // Verify it's gone
        var after = await Db.GetRecipeByIdAsync(recipeId);
        Assert.IsNull(after);
    }

    [TestMethod]
    public async Task UpdateAsync_UpdatesRecipe()
    {
        // Create a recipe to update
        var createDto = new RecipeCreateDto
        {
            OwnerId = TestUserId,
            Name = "Recipe Before Update",
            Description = "Original description",
            Servings = 2,
            Components = new List<ComponentCreateDto>()
        };
        var recipeId = await Db.CreateRecipeAsync(createDto);

        // Update it
        var updateDto = new RecipeUpdateDto
        {
            Id = recipeId,
            OwnerId = TestUserId,
            Name = "Recipe After Update",
            Description = "Updated description",
            Servings = 8,
            Components = new List<ComponentCreateDto>()
        };
        await Db.UpdateRecipeAsync(updateDto);

        // Verify the update
        var result = await Db.GetRecipeByIdAsync(recipeId);
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
            OwnerId = TestUserId,
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
                            Ingredient = new IngredientRefDto { Id = _testIngredientId, Name = TEST_INGREDIENT_NAME },
                            Quantity = 2.0,
                            Unit = "tablespoon",
                            Position = 1
                        }
                    }
                }
            }
        };
        await Db.CreateRecipeAsync(createDto);

        var result = await Db.SearchRecipesAsync(TEST_INGREDIENT_NAME);

        Assert.IsNotNull(result);
        Assert.IsGreaterThanOrEqualTo(1, result.Count);
        Assert.IsTrue(result.Any(r => r.Name == "Recipe With Test Ingredient"));
    }
}
