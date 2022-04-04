
namespace babe_algorithms.Models;
public class RecipeNutritionFacts
{
    public RecipeNutritionFacts()
    {
        Components = new List<NutritionFactVector>();
    }

    public NutritionFactVector Recipe { get; set; }
    public List<NutritionFactVector> Components { get; set; }
}

public class NutritionFactVector
{
    public double Calories { get; set; }
    // public Unit  { get; set; }
    // public Unit ServingUnit { get; set; }

    public NutritionFactVector Combine(NutritionFactVector nf) =>
        new()
        {
            Calories = this.Calories + nf.Calories,
        };
    
    public static NutritionFactVector operator +(
        NutritionFactVector a,
        NutritionFactVector b) =>
            a.Combine(b);
}