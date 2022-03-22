using Newtonsoft.Json.Converters;

namespace babe_algorithms.Models;
public class Ingredient
{
    [Required]
    public string Name { get; set; }
    // public MeasureType MeasureType { get; set; }
    public Guid Id { get; set; }

    [JsonIgnore]
    public StandardReferenceNutritionData NutritionData { get; set; }
}

[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum Unit
{
    // Volumetric Units
    Tablespoon = 100,
    Teaspoon = 101,
    Milliliter = 102,
    Cup = 103,
    FluidOunce = 104,
    Pint = 105,
    Quart = 106,
    Gallon = 107,
    Liter = 108,

    // Count
    Count = 1000,

    // Mass
    Ounce = 2000,
    Pound = 2001,
    Milligram = 2002,
    Gram = 2003,
    Kilogram = 2004
}

public static class UnitExtension {
    public static double GetSIValue(this Unit value) {
        return value switch
        {
            // Volume
            Unit.Tablespoon => 0.0147868,
            Unit.Teaspoon => 0.00492892,
            Unit.Milliliter => 0.0001,
            Unit.Cup => 0.236588,
            Unit.FluidOunce => 0.0295735,
            Unit.Pint  => 0.568261,
            Unit.Quart => 0.946353,
            Unit.Gallon => 3.78541,
            Unit.Liter => 1.0,
            // Count
            Unit.Count => 1.0,
            // Weight
            Unit.Ounce => 0.0283495,
            Unit.Pound => 0.453592,
            Unit.Milligram => 0.0001,
            Unit.Gram => 0.001,
            Unit.Kilogram => 1.0,
        };
    }
}

public class Category
{
    [Required]
    public string Name { get; set; }
    public Guid Id { get; set; }
    [Newtonsoft.Json.JsonIgnore]
    public ICollection<Recipe> Recipes { get; set; }
}