using System.Text.Json.Serialization;

namespace BabeAlgorithms.Models.Contracts;

public class RecipeStepDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("text")]
    public string Text { get; set; } = null!;
}
