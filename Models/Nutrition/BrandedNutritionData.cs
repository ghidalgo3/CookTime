using System.Text.Json;

namespace babe_algorithms.Models;

#nullable enable

public class BrandedNutritionData : USDANutritionData
{
    [Key]
    public string GtinUpc { get; set; }

    public string Ingredients { get; set; }

    public double ServingSize { get; set; }

    public string ServingSizeUnit { get; set; }

    [JsonIgnore]
    public JsonDocument LabelNutrients { get; set; }

    public string BrandedFoodCategory { get; set; }

    public override double CalculateDensity() => 1;

    public override double? CalculateUnitMass() {
        if (this.ServingSizeUnit.Equals("g"))
        {
            return this.ServingSize / 1000;
        }
        else
        {
            return 0.1;
        }
    }

    public override string GetCountModifier() => "";
}