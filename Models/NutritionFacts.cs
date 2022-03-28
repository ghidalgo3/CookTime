
namespace babe_algorithms.Models;
public class NutritionFacts
{
    public double Calories { get; set; }
    // public Unit  { get; set; }
    // public Unit ServingUnit { get; set; }

    public NutritionFacts Combine(NutritionFacts nf) =>
        new NutritionFacts()
        {
            Calories = this.Calories + nf.Calories,
        };
}