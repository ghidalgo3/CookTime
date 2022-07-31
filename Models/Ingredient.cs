#nullable enable
namespace babe_algorithms.Models;
using Newtonsoft.Json.Converters;

public class Ingredient
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; }

    public string CanonicalName => this.Name.Split(";").First();
    
    [JsonIgnore]
    public USDANutritionData? NormalNutritionData
    {
        get
        {
            if (this.NutritionData != null)
            {
                return this.NutritionData;
            }
            else
            {
                return this.BrandedNutritionData;
            }
        }
    }

    [JsonIgnore]
    public StandardReferenceNutritionData? NutritionData { get; set; }

    [JsonIgnore]
    public BrandedNutritionData? BrandedNutritionData { get; set; }

    /// <summary>
    /// Some ingredients in the USDA dataset (like shallots) do not have
    /// enough information to compute a unit mass.
    /// For such ingredients, we will simply manually track an expected unit mass.
    /// </summary>
    /// <value></value>
    public double ExpectedUnitMass { get; set; } = 0.1;

    public bool IsPlantBased 
    {
        get
        {
            if (this.NutritionData != null)
            {
                string foodCategoryDescription = this.NutritionData.GetFoodCategoryDescription();
                return StandardReferenceNutritionData.PlantBasedCategories.Any(cat => 
                {
                    return string.Equals(cat, foodCategoryDescription, StringComparison.InvariantCultureIgnoreCase);
                });
            }
            return false;
        }
    }
    public double? DensityKgPerL {
        get
        {
            return this.NormalNutritionData?.CalculateDensity();
        }
    }

    public override bool Equals(object obj)
    {
        return obj is Ingredient ingredient &&
               string.Equals(
                   Name.Trim().ToUpper(),
                   ingredient.Name.Trim().ToUpper(),
                   StringComparison.InvariantCultureIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name.Trim().ToUpper());
    }

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

    public static bool IsMass(this Unit value)
    {
        return (int)value >= 2000;
    }

    public static bool IsVolume(this Unit value)
    {
        return (int)value < 1000;
    }

    public static bool IsCount(this Unit value)
    {
        return (int)value < 2000 && (int)value >= 1000;
    }

    public static double GetSIValue(this Unit value) {
        return value switch
        {
            // Volume
            Unit.Tablespoon => 0.0147868,
            Unit.Teaspoon => 0.00492892,
            Unit.Milliliter => 0.001,
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
            Unit.Milligram => 0.001,
            Unit.Gram => 0.001,
            Unit.Kilogram => 1.0,
        };
    }
}