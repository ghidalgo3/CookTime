using System.Text.Json;
using BabeAlgorithms.Models.Contracts;

namespace CookTimeTests;

[TestClass]
public class TestRecipeSerialization
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    [TestMethod]
    public void RecipeDetailDto_SerializesToExpectedJson()
    {
        var recipe = new RecipeDetailDto
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Chocolate Chip Cookies",
            Description = "Classic homemade chocolate chip cookies",
            Owner = new OwnerDto
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                UserName = "baker123"
            },
            CooktimeMinutes = 12,
            CaloriesPerServing = 150,
            ServingsProduced = 24,
            Source = "https://example.com/cookies",
            StaticImage = "chocolate-chip-cookies.jpg",
            RecipeComponents = new List<ComponentDetailDto>
            {
                new()
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Name = "Cookie Dough",
                    Position = 1,
                    Ingredients = new List<IngredientRequirementDto>
                    {
                        new()
                        {
                            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                            Ingredient = new IngredientRefDto
                            {
                                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                                Name = "All-purpose flour",
                                IsNew = false
                            },
                            Text = "sifted",
                            Quantity = 2.25,
                            Unit = "cup",
                            Position = 1
                        },
                        new()
                        {
                            Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                            Ingredient = new IngredientRefDto
                            {
                                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                                Name = "Butter",
                                IsNew = false
                            },
                            Text = "softened",
                            Quantity = 1,
                            Unit = "cup",
                            Position = 2
                        },
                        new()
                        {
                            Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                            Ingredient = new IngredientRefDto
                            {
                                Id = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                                Name = "Chocolate chips",
                                IsNew = false
                            },
                            Quantity = 2,
                            Unit = "cup",
                            Position = 3
                        }
                    },
                    Steps = new List<string>
                    {
                        "Preheat oven to 375 degrees F.",
                        "Cream together butter and sugars until fluffy.",
                        "Mix in flour and chocolate chips.",
                        "Drop rounded tablespoons onto baking sheet.",
                        "Bake for 9-11 minutes until golden brown."
                    }
                }
            },
            Categories = new List<CategoryDto>
            {
                new() { CategoryId = 1, Name = "Dessert", Slug = "dessert" },
                new() { CategoryId = 2, Name = "Baking", Slug = "baking" }
            },
            ReviewCount = 42,
            AverageReviews = 4.7
        };

        var json = JsonSerializer.Serialize(recipe, JsonOptions);

        Console.WriteLine("RecipeDetailDto JSON:");
        Console.WriteLine(json);

        var expected = """
            {
              "id": "11111111-1111-1111-1111-111111111111",
              "name": "Chocolate Chip Cookies",
              "description": "Classic homemade chocolate chip cookies",
              "owner": {
                "id": "22222222-2222-2222-2222-222222222222",
                "userName": "baker123"
              },
              "cooktimeMinutes": 12,
              "caloriesPerServing": 150,
              "servingsProduced": 24,
              "source": "https://example.com/cookies",
              "staticImage": "chocolate-chip-cookies.jpg",
              "recipeComponents": [
                {
                  "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                  "name": "Cookie Dough",
                  "position": 1,
                  "ingredients": [
                    {
                      "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                      "ingredient": {
                        "id": "cccccccc-cccc-cccc-cccc-cccccccccccc",
                        "name": "All-purpose flour",
                        "isNew": false,
                        "densityKgPerL": null
                      },
                      "text": "sifted",
                      "quantity": 2.25,
                      "unit": "cup",
                      "position": 1
                    },
                    {
                      "id": "dddddddd-dddd-dddd-dddd-dddddddddddd",
                      "ingredient": {
                        "id": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
                        "name": "Butter",
                        "isNew": false,
                        "densityKgPerL": null
                      },
                      "text": "softened",
                      "quantity": 1,
                      "unit": "cup",
                      "position": 2
                    },
                    {
                      "id": "ffffffff-ffff-ffff-ffff-ffffffffffff",
                      "ingredient": {
                        "id": "11111111-2222-3333-4444-555555555555",
                        "name": "Chocolate chips",
                        "isNew": false,
                        "densityKgPerL": null
                      },
                      "text": null,
                      "quantity": 2,
                      "unit": "cup",
                      "position": 3
                    }
                  ],
                  "steps": [
                    "Preheat oven to 375 degrees F.",
                    "Cream together butter and sugars until fluffy.",
                    "Mix in flour and chocolate chips.",
                    "Drop rounded tablespoons onto baking sheet.",
                    "Bake for 9-11 minutes until golden brown."
                  ]
                }
              ],
              "categories": [
                {
                  "categoryId": 1,
                  "name": "Dessert",
                  "slug": "dessert"
                },
                {
                  "categoryId": 2,
                  "name": "Baking",
                  "slug": "baking"
                }
              ],
              "reviewCount": 42,
              "averageReviews": 4.7
            }
            """;

        Assert.AreEqual(expected, json);
    }

    [TestMethod]
    public void RecipeSummaryDto_SerializesToExpectedJson()
    {
        var summary = new RecipeSummaryDto
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Vegetable Stir Fry",
            Images = new List<ImageDto>
            {
                new()
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "stir-fry-main.jpg"
                },
                new()
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Name = "stir-fry-closeup.jpg"
                }
            },
            Categories = new List<string> { "Dinner", "Healthy", "Quick" },
            AverageReviews = 4.2,
            ReviewCount = 15,
            IsFavorite = true
        };

        var json = JsonSerializer.Serialize(summary, JsonOptions);

        Console.WriteLine("RecipeSummaryDto JSON:");
        Console.WriteLine(json);

        var expected = """
            {
              "id": "33333333-3333-3333-3333-333333333333",
              "name": "Vegetable Stir Fry",
              "images": [
                {
                  "id": "44444444-4444-4444-4444-444444444444",
                  "name": "stir-fry-main.jpg"
                },
                {
                  "id": "55555555-5555-5555-5555-555555555555",
                  "name": "stir-fry-closeup.jpg"
                }
              ],
              "categories": [
                "Dinner",
                "Healthy",
                "Quick"
              ],
              "averageReviews": 4.2,
              "reviewCount": 15,
              "isFavorite": true
            }
            """;

        Assert.AreEqual(expected, json);
    }

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
                { "id": "44444444-4444-4444-4444-444444444444", "name": "salad.jpg" }
            ],
            "categories": ["Lunch", "Healthy"],
            "averageReviews": 4.0,
            "reviewCount": 10,
            "isFavorite": false
        }
        """;

        var summary = JsonSerializer.Deserialize<RecipeSummaryDto>(json, JsonOptions);

        Assert.IsNotNull(summary);
        Assert.AreEqual("Quick Salad", summary.Name);
        Assert.AreEqual(1, summary.Images.Count);
        Assert.AreEqual("salad.jpg", summary.Images[0].Name);
        Assert.AreEqual(2, summary.Categories.Count);
        Assert.IsTrue(summary.Categories.Contains("Lunch"));
        Assert.AreEqual(false, summary.IsFavorite);
    }
}
