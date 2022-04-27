using System.Text.Json;

#nullable enable

namespace babe_algorithms.Models;

public abstract class USDANutritionData 
{
    public int FdcId { get; set; }

    public string Description { get; set; }

    [JsonIgnore]
    public JsonDocument FoodNutrients { get; set; }

    // public Ingredient? Ingredient { get; set; }

    public abstract double CalculateDensity();

    public abstract double? CalculateUnitMass();

    public abstract string GetCountModifier();
}