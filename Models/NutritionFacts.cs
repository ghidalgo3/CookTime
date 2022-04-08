
namespace babe_algorithms.Models;
public class RecipeNutritionFacts
{
    public RecipeNutritionFacts()
    {
        Components = new List<NutritionFactVector>();
    }

    public NutritionFactVector Recipe { get; set; }
    public List<NutritionFactVector> Components { get; set; }
    public List<IngredientNutritionDescription> Ingredients { get; set; }
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
        };
    
    public static NutritionFactVector operator +(
        NutritionFactVector a,
        NutritionFactVector b) =>
            a.Combine(b);
}