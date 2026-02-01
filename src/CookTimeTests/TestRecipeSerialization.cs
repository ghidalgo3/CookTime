using System.Text.Json;
using BabeAlgorithms.Models.Contracts;

namespace CookTime.Test;

[TestClass]
public class TestRecipeSerialization
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
  };

  [TestMethod]
  public void RecipeDetailDto_DeserializesFromJson()
  {
    var json = """
        {
            "id": "11111111-1111-1111-1111-111111111111",
            "name": "Test Recipe",
            "description": "A test",
            "owner": {
                "id": "22222222-2222-2222-2222-222222222222",
                "userName": "testuser"
            },
            "cooktimeMinutes": 30,
            "caloriesPerServing": 200,
            "servingsProduced": 4,
            "source": null,
            "staticImage": "test.jpg",
            "recipeComponents": [],
            "categories": [],
            "reviewCount": 5,
            "averageReviews": 3.5
        }
        """;

    var recipe = JsonSerializer.Deserialize<RecipeDetailDto>(json, JsonOptions);

    Assert.IsNotNull(recipe);
    Assert.AreEqual("Test Recipe", recipe.Name);
    Assert.AreEqual(30, recipe.CooktimeMinutes);
    Assert.AreEqual(200, recipe.CaloriesPerServing);
    Assert.AreEqual(4, recipe.ServingsProduced);
    Assert.AreEqual("test.jpg", recipe.StaticImage);
    Assert.AreEqual(5, recipe.ReviewCount);
    Assert.AreEqual(3.5, recipe.AverageReviews);
    Assert.IsNotNull(recipe.Owner);
    Assert.AreEqual("testuser", recipe.Owner.UserName);
  }

  [TestMethod]
  public void RecipeSummaryDto_DeserializesFromJson()
  {
    var json = """
        {
            "id": "33333333-3333-3333-3333-333333333333",
            "name": "Quick Salad",
            "images": [
                { "id": "44444444-4444-4444-4444-444444444444", "url": "https://example.com/salad.jpg" }
            ],
            "categories": ["Lunch", "Healthy"],
            "averageReviews": 4.0,
            "reviewCount": 10
        }
        """;

    var summary = JsonSerializer.Deserialize<RecipeSummaryDto>(json, JsonOptions);

    Assert.IsNotNull(summary);
    Assert.AreEqual("Quick Salad", summary.Name);
    Assert.HasCount(1, summary.Images);
    Assert.AreEqual("https://example.com/salad.jpg", summary.Images[0].Url);
    Assert.HasCount(2, summary.Categories);
    Assert.Contains("Lunch", summary.Categories);
  }
}
