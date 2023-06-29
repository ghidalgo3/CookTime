using babe_algorithms.Models;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms.Tests;

[TestClass]
public class ImageToRecipeTests
{
    public static IComputerVision GetComputerVision()
    {
        return new AzureCognitiveServices(new AzureOptions()
        {
            VisionEndpoint = "https://cooktime.cognitiveservices.azure.com/",
            VisionKey = Environment.GetEnvironmentVariable("Azure__VisionKey"),
        });
    }

    [TestMethod]
    [Ignore("Ignore tests that make network calls to 3rd party APIs.")]
    public async Task LemonyPastaWithChickpeas()
    {
        var ai = ChatGPTTests.GetChatGPT();
        var cv = GetComputerVision();

        var fileStream = File.OpenRead("Images/lemony pasta with chickpeas.png");
        var text = await cv.GetTextAsync(fileStream, CancellationToken.None);
        var recipe = await ai.ConvertToRecipeAsync(text, CancellationToken.None);
        Assert.IsNotNull(recipe);
        Assert.AreEqual("Lemony Pasta With Chickpeas", recipe.Name);
        Assert.AreEqual(4, recipe.ServingsProduced);
        Assert.AreEqual(3, recipe.RecipeComponents[0].Steps.Count);
        Assert.AreEqual(12, recipe.RecipeComponents[0].Ingredients.Count);
        Assert.AreEqual(0.5, recipe.RecipeComponents[0].Ingredients[0].Quantity);
    }

    [TestMethod]
    public async Task InsertEmptyRecipe()
    {
        var context = Mocks.GetApplicationDbContext();
        Mocks.SeedDatabaseIngredients(context);
        var oliveOilIngredient = context.Ingredients.Where(i => EF.Functions.ILike(i.Name, "olive oil")).FirstOrDefault();
        var initialIngredientCount = context.Ingredients.Count();
        var recipe = new MultiPartRecipe()
        {
            Name = "Test",
            RecipeComponents = new List<RecipeComponent>
            {
                new RecipeComponent
                {
                    Ingredients = new List<MultiPartIngredientRequirement>
                    {
                        new MultiPartIngredientRequirement
                        {
                            Ingredient = new Ingredient()
                            {
                                Name = "olive oil",
                            },
                            Quantity = 1,
                            Unit = Unit.Tablespoon
                        }
                    }
                }
            }
        };
        Assert.AreEqual(Guid.Empty, recipe.RecipeComponents[0].Ingredients[0].Ingredient.Id, "Initial ingredient GUID is empty");
        await context.LinkImportedRecipeIngredientsAsync(recipe);
        Assert.AreNotEqual(Guid.Empty, recipe.RecipeComponents[0].Ingredients[0].Ingredient.Id, "After linking, ID should match existing ingredient");
        context.MultiPartRecipes.Add(recipe);
        context.SaveChanges();
        Assert.AreEqual(initialIngredientCount, context.Ingredients.Count(), "No new ingredients should have been created / existing olive oil should have been used.");
    }
}