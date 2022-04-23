
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace babe_algorithms.Models;

public class StandardReferenceNutritionData : USDANutritionData
{
    [Key]
    public int NdbNumber { get; set; }

    [JsonIgnore]
    public JsonDocument NutrientConversionFactors { get; set; }

    [JsonIgnore]
    public JsonDocument FoodCategory { get; set; }

    [JsonIgnore]
    public JsonDocument FoodPortions { get; set; }

    public string CountRegex { get; set; }

    /// <summary>
    /// Calculates the density of an ingredient in kilograms per liter.
    /// </summary>
    /// <remarks>
    /// The USDA data is all normalized to 100g of a particular ingredient.
    /// Not all ingredients are measured by mass in practice,
    /// some are counts of some kind and some are volumes.
    /// The density information in encoded in the "food portions" part of the
    /// SR data. This method attempts to calculate a density based on 
    /// the different portions in the data set.
    /// 
    /// For example NDB 1077 (whole milk) has known portions of 1 cup
    /// with gram weight of 244g. We know that 1 cup is ~0.236 L
    /// and we know that 1 cup of milk is 244g, which allows us to compute
    /// a density of 244g / 0.236L * 1kg/1000g == 1.033 kg/L.
    /// </remarks>
    /// <returns></returns>
    public override double CalculateDensity()
    {
        var foodPortions = JArray.Parse(this.FoodPortions.RootElement.GetRawText());
        foreach (var portion in foodPortions)
        {
            if (TryParseUnit(portion["modifier"].Value<string>(), null, out Unit unit))
            {
                if (unit.IsVolume())
                {
                    return portion["gramWeight"].Value<double>() / unit.GetSIValue() / 1000.0;
                }
            }
        }
        return 1.0;
    }

    public override double CalculateUnitMass()
    {
        var foodPortions = JArray.Parse(this.FoodPortions.RootElement.GetRawText());
        foreach (var portion in foodPortions)
        {
            if (TryParseUnit(portion["modifier"].Value<string>(), this, out Unit unit))
            {
                if (unit.IsCount())
                {
                    return portion["gramWeight"].Value<double>() / 1000.0;
                }
            }
        }

        return 1.0;
    }

    public override string GetCountModifier()
    {
        var foodPortions = JArray.Parse(this.FoodPortions.RootElement.GetRawText());
        foreach (var portion in foodPortions)
        {
            var modifier = portion["modifier"].Value<string>();
            if (TryParseUnit(modifier, this, out Unit unit))
            {
                if (unit.IsCount())
                {
                    return modifier;
                }
            }
        }

        return string.Empty;
    }

    public static bool TryParseUnit(string modifier, StandardReferenceNutritionData srData, out Unit unit)
    {
        unit = Unit.Count;
        var normalize = modifier.ToUpperInvariant();
        switch (normalize)
        {
            // Volume
            case "FL OZ":
                unit = Unit.FluidOunce;
                return true;
            case "QUART":
                unit = Unit.Quart;
                return true;
            case "TBSP":
                unit = Unit.Tablespoon;
                return true;
            case "TSP":
                unit = Unit.Teaspoon;
                return true;
            case "CUP":
                unit = Unit.Cup;
                return true;
            // mass
            // count
            default:
                break;
        }

        if (normalize.Contains("CUP"))
        {
            unit = Unit.Cup;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(srData?.CountRegex))
        {
            if (Regex.IsMatch(modifier, srData.CountRegex, RegexOptions.IgnoreCase))
            {
                unit = Unit.Count;
                return true;
            }
        }

        return false;
    }

    public JObject ToJObject()
    {
        var result = new JObject
        {
            ["ndbNumber"] = this.NdbNumber,
            ["description"] = this.Description,
            ["fdcId"] = this.FdcId,
            ["foodNutrients"] = JToken.Parse(this.FoodNutrients.RootElement.GetRawText()),
            ["nutrientConversionFactors"] = JToken.Parse(this.NutrientConversionFactors.RootElement.GetRawText()),
            ["foodCategory"] = JToken.Parse(this.FoodCategory.RootElement.GetRawText()),
            ["foodPortions"] = JToken.Parse(this.FoodPortions.RootElement.GetRawText())
        };
        return result;
    }
}
