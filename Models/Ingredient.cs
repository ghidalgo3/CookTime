#nullable enable
namespace babe_algorithms.Models;
using Newtonsoft.Json.Converters;

public class Ingredient
{
    public Guid Id { get; set; }

    [Required]
    public required string Name { get; set; }

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

    public override bool Equals(object? obj)
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