using System.Text.Json.Serialization;

namespace BabeAlgorithms.Models.Contracts;

public class RecipeStepDto
{
    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("instruction")]
    public string Instruction { get; set; } = null!;
}
