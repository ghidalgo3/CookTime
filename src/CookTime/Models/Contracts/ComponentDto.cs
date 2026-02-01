using System.Text.Json.Serialization;

namespace BabeAlgorithms.Models.Contracts;

public class ComponentDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("ingredients")]
    public List<IngredientRequirementDto> Ingredients { get; set; } = new();

    [JsonPropertyName("steps")]
    public List<string>? Steps { get; set; }
}

public class ComponentDetailDto : ComponentDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
}
