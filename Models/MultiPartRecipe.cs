using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;

namespace babe_algorithms.Models;

public class MultiPartRecipe : IImageContainer
{
    public MultiPartRecipe() 
    {
        
    }
    public MultiPartRecipe(Recipe recipe) 
    {
        this.Name = recipe.Name;
        this.StaticImage = recipe.StaticImage;
        this.ServingsProduced = recipe.ServingsProduced;
        this.Cooktime = recipe.Cooktime;
        this.CaloriesPerServing = recipe.CaloriesPerServing;
        // this.Id = recipe.Id;
        this.Categories = recipe.Categories;
        this.Images = recipe.Images;
        this.Source = recipe.Source;
        this.RecipeComponents = new List<RecipeComponent>() 
        {
            new RecipeComponent() 
            {
                Name = recipe.Name,
                Ingredients = recipe.Ingredients.Select(ir => new MultiPartIngredientRequirement(ir)).ToList(),
                Steps = recipe.Steps.Select(s => new MultiPartRecipeStep(s)).ToList(),
            },
        };

    }

    [Required]
    public string Name { get; set; }
    public string StaticImage { get; set; }
    public double ServingsProduced { get; set; } = 1.0;
    public TimeSpan Cooktime { get; set; }
    public double CaloriesPerServing { get; set; }
    public Guid Id { get; set; }
    public ISet<Category> Categories { get; set; }
    public List<Image> Images { get; set; }
    public List<RecipeComponent> RecipeComponents { get; set; } = new List<RecipeComponent>();
    /// <summary>
    /// The source where the recipe came from.
    /// </summary>
    public string Source { get; set; }
    [JsonIgnore]
    public NpgsqlTsVector SearchVector { get; set; }
}

public class RecipeComponent : IRecipeComponent<MultiPartRecipeStep, MultiPartIngredientRequirement>
{
    public string Name { get; set; }
    public Guid Id { get; set; }
    public int Position { get; set; }
    // public Guid MultiPartRecipeId { get; set; }
    public List<MultiPartIngredientRequirement> Ingredients { get; set; } = new List<MultiPartIngredientRequirement>();
    public List<MultiPartRecipeStep> Steps { get; set; } = new List<MultiPartRecipeStep>();

    public bool IsEmpty() {
        return string.IsNullOrWhiteSpace(this.Name) && this.Ingredients.Count == 0 && this.Steps.Count == 0;
    }
}

[Owned]
public class MultiPartRecipeStep : IRecipeStep
{
    public MultiPartRecipeStep(){}
    public MultiPartRecipeStep(RecipeStep step)
    {
        this.Text = step.Text;
    }
    public string Text { get; set; }
}

[Owned]
public class MultiPartIngredientRequirement : IIngredientRequirement
{
    public MultiPartIngredientRequirement()
    {
    }

    public MultiPartIngredientRequirement(IngredientRequirement ir)
    {
        this.Ingredient = ir.Ingredient;
        this.Unit = ir.Unit;
        this.Quantity = ir.Quantity;
        this.Position = ir.Position;
    }

    public Guid Id { get; set; }

    public Ingredient Ingredient { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public Unit Unit { get; set; }

    public double Quantity { get; set; }

    /// <summary>
    /// The position this ingredient should be placed in.
    /// </summary>
    public int Position { get; set; }

    public NutritionFacts CalculateNutritionFacts()
    {
        var nutritionFacts = new NutritionFacts();
        if (this.Ingredient.NutritionData == null)
        {
            return nutritionFacts;
        }
        // find the food nutrient 
        var nutritionData = JToken.Parse(this.Ingredient.NutritionData.FoodNutrients.RootElement.GetRawText());
        // this represents calories in 100 grams of this ingredient
        var calorieData =  nutritionData.SelectTokens(@"$[?(@.nutrient.name == 'Energy' && @.nutrient.unitName == 'kcal')]").First();
        if (this.Unit.IsMass())
        {
            var kilgramsOfUnit = this.Unit.GetSIValue() * this.Quantity;
            // * 10 because the SR data is for 100g 
            nutritionFacts.Calories = kilgramsOfUnit * 10 * calorieData.Value<double>("amount");
        }
        return nutritionFacts;
    }
}