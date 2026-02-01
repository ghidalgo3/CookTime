using System.Text.Json.Serialization;

namespace BabeAlgorithms.Models.Contracts;

public class NutritionDataDto
{
    [JsonPropertyName("nutritionFactsId")]
    public int? NutritionFactsId { get; set; }

    [JsonPropertyName("dataSource")]
    public string? DataSource { get; set; }

    [JsonPropertyName("sourceId")]
    public string? SourceId { get; set; }

    [JsonPropertyName("servingSize")]
    public decimal? ServingSize { get; set; }

    [JsonPropertyName("servingSizeUnit")]
    public string? ServingSizeUnit { get; set; }

    [JsonPropertyName("calories")]
    public decimal? Calories { get; set; }

    [JsonPropertyName("totalFat")]
    public decimal? TotalFat { get; set; }

    [JsonPropertyName("saturatedFat")]
    public decimal? SaturatedFat { get; set; }

    [JsonPropertyName("transFat")]
    public decimal? TransFat { get; set; }

    [JsonPropertyName("cholesterol")]
    public decimal? Cholesterol { get; set; }

    [JsonPropertyName("sodium")]
    public decimal? Sodium { get; set; }

    [JsonPropertyName("totalCarbohydrate")]
    public decimal? TotalCarbohydrate { get; set; }

    [JsonPropertyName("dietaryFiber")]
    public decimal? DietaryFiber { get; set; }

    [JsonPropertyName("totalSugars")]
    public decimal? TotalSugars { get; set; }

    [JsonPropertyName("addedSugars")]
    public decimal? AddedSugars { get; set; }

    [JsonPropertyName("protein")]
    public decimal? Protein { get; set; }

    [JsonPropertyName("vitaminD")]
    public decimal? VitaminD { get; set; }

    [JsonPropertyName("calcium")]
    public decimal? Calcium { get; set; }

    [JsonPropertyName("iron")]
    public decimal? Iron { get; set; }

    [JsonPropertyName("potassium")]
    public decimal? Potassium { get; set; }
}
