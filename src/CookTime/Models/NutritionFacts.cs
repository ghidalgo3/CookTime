using Newtonsoft.Json.Linq;

namespace CookTime.Models;

/// <summary>
/// DTO for presenting nutrition information
/// about a recipe.
/// </summary>
public record RecipeNutritionFacts
{
    public required NutritionFactVector Recipe { get; set; }
    public List<NutritionFactVector> Components { get; set; } = [];
    public List<DietDetail> DietDetails { get; set; } = [];
    public List<IngredientNutritionDescription> Ingredients { get; set; } = [];
}

public enum DietOpinion
{
    Recommended,
    Allowed,
    Neutral,
    Discouraged,
    Forbidden,
}

// examples:
// Keto
// Low-carb
// Paleo
// Daily ten
public class DietDetail
{
    /// <summary>
    /// The name of the diet this diet detail describes.
    /// For example, TodaysTen is a valid value here.
    /// </summary>
    /// <value></value>
    public required string Name { get; set; }

    public DietOpinion Opinion { get; set; } = DietOpinion.Neutral;

    /// <summary>
    /// Since every recipe is a unique snowflake, just dump whatever you want in here
    /// and make sure it's JSON serializable.
    /// </summary>
    /// <value></value>
    public required object Details { get; set; }
}

public class IngredientNutritionDescription
{
    public required string NutritionDatabaseId { get; set; }
    public required string NutritionDatabaseDescriptor { get; set; }
    public required string Name { get; set; }
    public required string Unit { get; set; }
    public required double Quantity { get; set; }
    public required string Modifier { get; set; }
    public required double CaloriesPerServing { get; set; }
}

public class NutritionFactVector
{
    public double Calories { get; set; }
    public double Carbohydrates { get; set; }
    public double SaturatedFats { get; set; }
    public double MonoUnsaturatedFats { get; set; }
    public double PolyUnsaturatedFats { get; set; }
    public double TransFats { get; set; }
    public double Proteins { get; set; }
    public double Sugars { get; set; }
    public double Iron { get; set; }
    public double VitaminD { get; set; }
    public double Potassium { get; set; }
    public double Calcium { get; set; }

    public NutritionFactVector Combine(NutritionFactVector nf) =>
        new()
        {
            Calories = this.Calories + nf.Calories,
            Carbohydrates = this.Carbohydrates + nf.Carbohydrates,
            SaturatedFats = this.SaturatedFats + nf.SaturatedFats,
            MonoUnsaturatedFats = this.MonoUnsaturatedFats + nf.MonoUnsaturatedFats,
            PolyUnsaturatedFats = this.PolyUnsaturatedFats + nf.PolyUnsaturatedFats,
            TransFats = this.TransFats + nf.TransFats,
            Proteins = this.Proteins + nf.Proteins,
            Sugars = this.Sugars + nf.Sugars,
            Iron = this.Iron + nf.Iron,
            VitaminD = this.VitaminD + nf.VitaminD,
            Calcium = this.Calcium + nf.Calcium,
            Potassium = this.Potassium + nf.Potassium
        };

    public static NutritionFactVector operator +(
        NutritionFactVector a,
        NutritionFactVector b) =>
            a.Combine(b);

    public static NutritionFactVector operator /(NutritionFactVector a, double divisor)
    {
        if (divisor == 0)
        {
            divisor = 1;
        }

        return new NutritionFactVector
        {
            Calories = a.Calories / divisor,
            Carbohydrates = a.Carbohydrates / divisor,
            SaturatedFats = a.SaturatedFats / divisor,
            MonoUnsaturatedFats = a.MonoUnsaturatedFats / divisor,
            PolyUnsaturatedFats = a.PolyUnsaturatedFats / divisor,
            TransFats = a.TransFats / divisor,
            Proteins = a.Proteins / divisor,
            Sugars = a.Sugars / divisor,
            Iron = a.Iron / divisor,
            VitaminD = a.VitaminD / divisor,
            Calcium = a.Calcium / divisor,
            Potassium = a.Potassium / divisor
        };
    }
}