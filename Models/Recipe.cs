using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;

namespace babe_algorithms;

public class Recipe : IImageContainer, IRecipeComponent<RecipeStep, IngredientRequirement>
{
    [Required]
    public string Name { get; set; }
    public string StaticImage { get; set; }
    public List<IngredientRequirement> Ingredients { get; set; }
    public List<RecipeStep> Steps { get; set; }
    public double ServingsProduced { get; set; } = 1.0;
    public TimeSpan Cooktime { get; set; }
    public double CaloriesPerServing { get; set; }
    public Guid Id { get; set; }
    public ISet<Category> Categories { get; set; }
    public List<Image> Images { get; set; }

    /// <summary>
    /// The source where the recipe came from.
    /// </summary>
    public string Source { get; set; }
}

[Owned]
public class RecipeStep : IRecipeStep
{
    public string Text { get; set; }
}

[Owned]
public class IngredientRequirement : IIngredientRequirement
{
    public Guid Id { get; set; }

    public Ingredient Ingredient { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public Unit Unit { get; set; }

    public double Quantity { get; set; }

    /// <summary>
    /// The position this ingredient should be placed in.
    /// </summary>
    public int Position { get; set; }

    public NutritionFactVector CalculateNutritionFacts()
    {
        throw new NotImplementedException();
    }
}