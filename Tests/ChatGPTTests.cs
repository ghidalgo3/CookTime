using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace babe_algorithms;

[TestClass]
// [Ignore]
public class ChatGPTTests
{
    [TestMethod]
    public void Smoke()
    {
        IRecipeArtificialIntelligence ai = new ChatGPT(new OpenAIOptions()
        {
            Key = ""
        }, null);
        Assert.IsTrue(ai.ConvertToRecipeAsync("5 grams of salt\n1/4 cups of milk\n10 milliliters water", CancellationToken.None).Result != null);
    }
}