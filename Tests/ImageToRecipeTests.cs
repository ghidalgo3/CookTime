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

    // [Ignore("Ignore tests that make network calls to 3rd party APIs.")]
    [DataTestMethod]
    // [DataRow("Images/lemony pasta with chickpeas.png", "Lemony Pasta With Chickpeas", 12, 3)]
    // [DataRow("Images/breville-air-fryer.png", "Dehydrated Bananas With Coconut", 1, 4)]
    [DataRow("Images/hot apple waffle.png", "Hot Apple Pie And Coconut Crumble", 14, 8)]
    public async Task LemonyPastaWithChickpeas(string file, string title, int ingredients, int steps)
    {
        var ai = ChatGPTTests.GetChatGPT();
        var cv = GetComputerVision();
        var fileStream = File.OpenRead(file);
        var text = await cv.GetTextAsync(fileStream, CancellationToken.None);
        var recipe = await ai.ConvertToRecipeAsync(text, CancellationToken.None);
        Assert.IsNotNull(recipe);
        Assert.AreEqual(title, recipe.Name);
        // Assert.AreEqual(4, recipe.ServingsProduced);
        Assert.AreEqual(steps, recipe.RecipeComponents[0].Steps.Count);
        Assert.AreEqual(ingredients, recipe.RecipeComponents[0].Ingredients.Count);
        // Assert.AreEqual(0.5, recipe.RecipeComponents[0].Ingredients[0].Quantity);
    }

    [TestMethod]
    [Ignore]
    public async Task ReorderedTextTest()
    {

        var ai = ChatGPTTests.GetChatGPT();
        var text =
"""
Hot apple pie and coconut crumble
Ingredients
4 eggs
2 cups milk
200g unsalted butter, melted and cooled 2 teaspoons vanilla extract
3 cups self-raising flour
1 teaspoon ground cinnamon
¼ cup caster sugar
¼ cup brown sugar
400g can pie apple
Crumble
2 tablespoons desiccated coconut
¼ cup rolled oats
¼ cup plain flour
¼ cup brown sugar
60g butter
Method
1. To make the crumble, combine coconut, oats, flour and sugar in a bowl. Using your fingertips, rub butter into mixture. Heat a large frying pan over medium high heat. Add oat mixture and cook
8-10 minutes, stirring frequently until golden, crispy and crumbly. Remove and cool completely.
2. To make the waffles, place eggs, milk, butter and vanilla in a large jug and whisk until well combined.
3. Combine flour, cinnamon and sugars into a large mixing bowl and make a well in the centre.
4. Carefully whisk in egg milk mixture to form a smooth batter. Fold through canned pie apple.
5. Select CLASSIC waffle setting and dial up number 6 on the browning control dial.
6. Preheat until orange light flashes up and the words HEATING disappear.
7. Using waffle dosing cup, pour ½ cup of batter into each waffle square. Close lid and cook until timer has finished and ready beep has sounded 3 times.
Repeat with remaining batter.
8. Serve with topped with crumble topping, vanilla ice cream and extra slices of apples.
""";

        var recipe = await ai.ConvertToRecipeAsync(text, CancellationToken.None);
        Assert.IsNotNull(recipe);
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