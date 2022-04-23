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
}