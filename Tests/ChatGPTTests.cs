using babe_algorithms.Models;

namespace babe_algorithms.Tests;

[TestClass]
// [Ignore]
public class ChatGPTTests
{

    public static IRecipeArtificialIntelligence GetChatGPT()
    {
        IRecipeArtificialIntelligence ai = new ChatGPT(new OpenAIOptions()
        {
            Key = Environment.GetEnvironmentVariable("OpenAI__Key")
        }, null);
        return ai;
    }

    [TestMethod]
    [Ignore]
    public async Task ImageGeneration()
    {
        var ai = GetChatGPT();
        await ai.GenerateRecipeImageAsync(new MultiPartRecipe()
        {
            Name = "Test Recipe",
        }, CancellationToken.None);
    }


    [TestMethod]
    // [Ignore("Ignore tests that make network calls to 3rd party APIs.")]
    public void NonRecipe()
    {
        var ai = GetChatGPT();
        var input =
"""
Not a recipe
""";

        var recipe = ai.ConvertToRecipeAsync(input, CancellationToken.None).Result;
        Assert.IsNotNull(recipe);
        // Assert.IsNotNull(recipe);
    }

    [TestMethod]
    // [Ignore("Ignore tests that make network calls to 3rd party APIs.")]
    public void SimpleRecipeParse()
    {
        var ai = GetChatGPT();
        var input =
"""
My crazy recipe
Serves 3
40 minutes to prepare

Ingredients:
5 grams kosher salt
1/4 cups of whole milk
10 milliliters water
4 garlic clove, minced

Steps:
Mix everything together in a blender.
Serve and enjoy.
""";

        var recipe = ai.ConvertToRecipeAsync(input, CancellationToken.None).Result;
        Assert.IsNotNull(recipe);
        Assert.AreEqual("My Crazy Recipe", recipe.Name);
        Assert.AreEqual(4, recipe.GetAllIngredients().Count);
        Assert.AreEqual(3, recipe.ServingsProduced);
        Assert.AreEqual(40, recipe.CooktimeMinutes);

        Assert.AreEqual(5, recipe.RecipeComponents[0].Ingredients[0].Quantity);
        Assert.AreEqual(Unit.Gram, recipe.RecipeComponents[0].Ingredients[0].Unit);
        Assert.AreEqual("kosher salt", recipe.RecipeComponents[0].Ingredients[0].Ingredient.Name);

        Assert.AreEqual(0.25, recipe.RecipeComponents[0].Ingredients[1].Quantity);
        Assert.AreEqual(Unit.Cup, recipe.RecipeComponents[0].Ingredients[1].Unit);
        Assert.AreEqual("whole milk", recipe.RecipeComponents[0].Ingredients[1].Ingredient.Name);

        Assert.AreEqual(10, recipe.RecipeComponents[0].Ingredients[2].Quantity);
        Assert.AreEqual(Unit.Milliliter, recipe.RecipeComponents[0].Ingredients[2].Unit);
        Assert.AreEqual("water", recipe.RecipeComponents[0].Ingredients[2].Ingredient.Name);

        Assert.AreEqual(4, recipe.RecipeComponents[0].Ingredients[3].Quantity);
        Assert.AreEqual(Unit.Count, recipe.RecipeComponents[0].Ingredients[3].Unit);
        Assert.AreEqual("garlic clove", recipe.RecipeComponents[0].Ingredients[3].Ingredient.Name);

        Assert.AreEqual(2, recipe.RecipeComponents[0].Steps.Count);
    }
}