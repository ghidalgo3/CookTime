using Newtonsoft.Json.Linq;

namespace babe_algorithms.Models;

/// <summary>
/// DTO for presenting nutrition information
/// about a recipe.
/// </summary>
public class RecipeNutritionFacts
{
    public RecipeNutritionFacts()
    {
        Components = new List<NutritionFactVector>();
        DietDetails = new List<DietDetail>();
    }

    public NutritionFactVector Recipe { get; set; }
    public List<NutritionFactVector> Components { get; set; }
    public List<DietDetail> DietDetails { get; set; }
    public List<IngredientNutritionDescription> Ingredients { get; set; }
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
    public string Name { get; set; }

    public DietOpinion Opinion { get; set; } = DietOpinion.Neutral;

    /// <summary>
    /// Since every recipe is a unique snowflake, just dump whatever you want in here
    /// and make sure it's JSON serializable.
    /// </summary>
    /// <value></value>
    public object Details { get; set; }
}

public class IngredientNutritionDescription
{
    public string nutritionDatabaseId { get; set; }
    public string NutritionDatabaseDescriptor { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public double Quantity { get; set; }
    public string Modifier { get; set; }
    public double CaloriesPerServing { get; set; }
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
}