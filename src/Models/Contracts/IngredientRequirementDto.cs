using System.Text.Json.Serialization;

namespace BabeAlgorithms.Models.Contracts;

public class IngredientRequirementDto
{
    [JsonPropertyName("ingredientId")]
    public int IngredientId { get; set; }

    [JsonPropertyName("ingredientName")]
    public string? IngredientName { get; set; }

    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("preparation")]
    public string? Preparation { get; set; }

    [JsonPropertyName("optional")]
    public bool Optional { get; set; }
}
