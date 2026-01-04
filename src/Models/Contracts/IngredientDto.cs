namespace BabeAlgorithms.Models.Contracts;

public class IngredientDto
{
    [JsonPropertyName("ingredientId")]
    public int IngredientId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("pluralName")]
    public string? PluralName { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("nutritionFacts")]
    public NutritionDataDto? NutritionFacts { get; set; }
}

public class IngredientCreateDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("pluralName")]
    public string? PluralName { get; set; }
}
