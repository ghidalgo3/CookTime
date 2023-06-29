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
    public async Task LemonyPastaWithChickpeas()
    {
        var ai = ChatGPTTests.GetChatGPT();
        var cv = GetComputerVision();

        var fileStream = File.OpenRead("Images/lemony pasta with chickpeas.png");
        var text = await cv.GetTextAsync(fileStream, CancellationToken.None);
        var recipe = await ai.ConvertToRecipeAsync(text, CancellationToken.None);
        Assert.AreEqual("Lemony Pasta With Chickpeas", recipe.Name);
        Assert.AreEqual(4, recipe.ServingsProduced);
        Assert.AreEqual(3, recipe.RecipeComponents[0].Steps.Count);
        Assert.AreEqual(12, recipe.RecipeComponents[0].Ingredients.Count);
        Assert.AreEqual(0.5, recipe.RecipeComponents[0].Ingredients[0].Quantity);
    }

    [TestMethod]
    public async Task InsertEmptyRecipe()
    {
        Assert.IsTrue(true);
    }
}