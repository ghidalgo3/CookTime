namespace BabeAlgorithms.Models.Contracts;

public class IngredientRequirementDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("ingredient")]
    public IngredientRefDto? Ingredient { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("quantity")]
    public double? Quantity { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }
}

public class IngredientRefDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("isNew")]
    public bool IsNew { get; set; }

    [JsonPropertyName("densityKgPerL")]
    public double? DensityKgPerL { get; set; }
}
