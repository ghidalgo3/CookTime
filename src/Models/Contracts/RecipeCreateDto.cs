using System.Text.Json.Serialization;

namespace BabeAlgorithms.Models.Contracts;

public class RecipeCreateDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = null!;

    [JsonPropertyName("prepTime")]
    public TimeSpan? PrepTime { get; set; }

    [JsonPropertyName("cookTime")]
    public TimeSpan? CookTime { get; set; }

    [JsonPropertyName("servings")]
    public int? Servings { get; set; }

    [JsonPropertyName("calories")]
    public int? Calories { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("recipeSource")]
    public string? RecipeSource { get; set; }

    [JsonPropertyName("components")]
    public List<ComponentDto> Components { get; set; } = new();

    [JsonPropertyName("steps")]
    public List<RecipeStepDto> Steps { get; set; } = new();

    [JsonPropertyName("categoryIds")]
    public List<int> CategoryIds { get; set; } = new();
}

public class RecipeUpdateDto : RecipeCreateDto
{
    [JsonPropertyName("recipeId")]
    public int RecipeId { get; set; }
}
