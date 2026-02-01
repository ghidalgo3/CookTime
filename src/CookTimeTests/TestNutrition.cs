using BabeAlgorithms.Models.Contracts;
using babe_algorithms.Services;
using Npgsql;
using NpgsqlTypes;
using Microsoft.AspNetCore.StaticAssets;

namespace Tests;

/// <summary>
/// Tests for the NutritionService using sample USDA data.
/// 
/// Sample data used:
/// - SR Legacy: "Pillsbury, Cinnamon Rolls with Icing, refrigerated dough" (ndbNumber: 18635)
///   Per 100g: 330 kcal, 4.34g protein, 53.4g carbs, 11.3g fat, 21.3g sugar, 28mg calcium, 1.93mg iron
///   Food portion: 44g per serving
/// 
/// Hand calculation for 100g of Cinnamon Rolls:
///   - Calories: 330 kcal
///   - Protein: 4.34g
///   - Carbohydrates: 53.4g
///   - Fat (total): 11.3g
///   - Saturated Fat: 3.25g
///   - Trans Fat: 4.29g
///   - Sugars: 21.3g
///   - Iron: 1.93mg
///   - Calcium: 28mg
/// </summary>
[TestClass]
public class TestNutrition : TestBase
{
    private static NutritionService _nutritionService = null!;
    private static Guid _testIngredientId;
    private static Guid? _nutritionFactsId;

    private static readonly string SampleDataDirectory = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "SampleData");

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        await InitializeAsync(nameof(TestNutrition));
        _nutritionService = new NutritionService(DataSource);

        await using var conn = await DataSource.OpenConnectionAsync();

        // Load sample SR Legacy nutrition data
        var srLegacyPath = Path.Combine(SampleDataDirectory, "sr_legacy_sample.ndjson");
        Console.WriteLine($"Loading SR Legacy sample from: {srLegacyPath}");
        var srCount = await LoadNutritionFactsAsync(conn, srLegacyPath, "usda_sr_legacy");
        Console.WriteLine($"  Loaded {srCount} SR Legacy nutrition facts");

        // Get the nutrition_facts_id for ndbNumber '18073' (White Bread)
        await using var getNfCmd = new NpgsqlCommand(
            "SELECT id FROM cooktime.nutrition_facts WHERE source_ids->>'ndbNumber' = '18073'", conn);
        _nutritionFactsId = await getNfCmd.ExecuteScalarAsync() as Guid?;

        // Create test ingredient linked to nutrition facts
        _testIngredientId = await CreateTestIngredientAsync("White bread", _nutritionFactsId, "slice");
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        await using var conn = await DataSource.OpenConnectionAsync();

        // Delete test recipes first (cascades to ingredient_requirements)
        await using var deleteRecipesCmd = new NpgsqlCommand(
            "DELETE FROM cooktime.recipes WHERE owner_id = $1", conn);
        deleteRecipesCmd.Parameters.AddWithValue(TestUserId);
        await deleteRecipesCmd.ExecuteNonQueryAsync();

        // Delete our test ingredient
        await using var deleteIngredientCmd = new NpgsqlCommand(
            "DELETE FROM cooktime.ingredients WHERE id = $1", conn);
        deleteIngredientCmd.Parameters.AddWithValue(_testIngredientId);
        await deleteIngredientCmd.ExecuteNonQueryAsync();

        // Clear any other ingredient references to our test nutrition facts, then delete
        await using var clearFkCmd = new NpgsqlCommand(
            "UPDATE cooktime.ingredients SET nutrition_facts_id = NULL WHERE nutrition_facts_id = $1", conn);
        clearFkCmd.Parameters.AddWithValue(_nutritionFactsId!.Value);
        await clearFkCmd.ExecuteNonQueryAsync();

        await using var deleteNfCmd = new NpgsqlCommand(
            "DELETE FROM cooktime.nutrition_facts WHERE id = $1", conn);
        deleteNfCmd.Parameters.AddWithValue(_nutritionFactsId!.Value);
        await deleteNfCmd.ExecuteNonQueryAsync();

        await CleanupAsync();
    }

    /// <summary>
    /// Test that nutrition facts are correctly loaded from sample data.
    /// </summary>
    [TestMethod]
    public void NutritionFacts_AreLoaded()
    {
        Assert.IsNotNull(_nutritionFactsId, "Nutrition facts should be loaded from sample data");
    }


    [TestMethod]
    public async Task RecipeWith100gWhiteBread()
    {
        var createDto = SimpleRecipe(1, [
            new IngredientRequirementCreateDto
            {
                Ingredient = new IngredientRefDto { Id = _testIngredientId, Name = "White bread" },
                Quantity = 100,
                Unit = "gram",
                Position = 1
            }
        ]);

        var recipeId = await Db.CreateRecipeAsync(createDto);

        // Act
        var nutrition = await _nutritionService.GetRecipeNutritionFactsAsync(recipeId);

        // Assert
        Assert.IsNotNull(nutrition, "Nutrition facts should be returned");

        // https://fdc.nal.usda.gov/food-details/174927/nutrients
        Assert.AreEqual(285, nutrition.Recipe.Calories, 1.0);
        // Assert.AreEqual(2.17, nutrition.Recipe.Proteins, 0.1, "Protein should be ~2.17g for 50g");
        // Assert.AreEqual(26.7, nutrition.Recipe.Carbohydrates, 0.1, "Carbohydrates should be ~26.7g for 50g");
    }

    [TestMethod]
    public async Task RecipeWithSliceWhiteBread()
    {
        var createDto = SimpleRecipe(1, [
            new IngredientRequirementCreateDto
            {
                Ingredient = new IngredientRefDto { Id = _testIngredientId, Name = "White bread" },
                Quantity = 1,
                Unit = "count",
                Position = 1
            }
        ]);

        var recipeId = await Db.CreateRecipeAsync(createDto);

        // Act
        var nutrition = await _nutritionService.GetRecipeNutritionFactsAsync(recipeId);

        // Assert
        Assert.IsNotNull(nutrition, "Nutrition facts should be returned");

        // https://fdc.nal.usda.gov/food-details/174927/nutrients
        Assert.AreEqual(120, nutrition.Recipe.Calories, 1.0);
        // Assert.AreEqual(2.17, nutrition.Recipe.Proteins, 0.1, "Protein should be ~2.17g for 50g");
        // Assert.AreEqual(26.7, nutrition.Recipe.Carbohydrates, 0.1, "Carbohydrates should be ~26.7g for 50g");
    }

    private static RecipeCreateDto SimpleRecipe(int servings, List<IngredientRequirementCreateDto> ingredients) =>
        new RecipeCreateDto
        {
            OwnerId = TestUserId,
            Name = "Test Recipe - 50g White Bread",
            Description = "Recipe for nutrition test with half amount",
            Servings = servings,
            CookingMinutes = 10,
            PrepMinutes = 5,
            Components = new List<ComponentCreateDto>
            {
                new ComponentCreateDto
                {
                    Name = "Main",
                    Position = 1,
                    Steps = new List<string> { "Heat and serve" },
                    Ingredients = ingredients
                }
            }
        };


    /// <summary>
    /// Simplified loader for test data - loads USDA nutrition facts from JSON file.
    /// </summary>
    private static async Task<int> LoadNutritionFactsAsync(NpgsqlConnection conn, string filePath, string dataset)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"  Warning: {filePath} not found, skipping {dataset} nutrition facts");
            return 0;
        }

        var count = 0;
        var lineNumber = 0;

        await foreach (var line in File.ReadLinesAsync(filePath))
        {
            lineNumber++;
            // Skip empty lines or the closing bracket
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // Remove trailing comma if present
            var jsonLine = line.TrimEnd().TrimEnd(',');

            try
            {
                await using var cmd = new NpgsqlCommand("SELECT cooktime.import_nutrition_facts($1::jsonb, $2)", conn);
                cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, jsonLine);
                cmd.Parameters.AddWithValue(NpgsqlDbType.Text, dataset);

                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    count++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR on line {lineNumber}: {ex.Message}");
            }
        }

        return count;
    }
}
