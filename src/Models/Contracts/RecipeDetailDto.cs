using System.Text.Json.Serialization;

namespace BabeAlgorithms.Models.Contracts;

public class RecipeDetailDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("ownerId")]
    public Guid? OwnerId { get; set; }

    [JsonPropertyName("prepMinutes")]
    public double? PrepMinutes { get; set; }

    [JsonPropertyName("cookingMinutes")]
    public double? CookingMinutes { get; set; }

    [JsonPropertyName("servings")]
    public int? Servings { get; set; }

    [JsonPropertyName("calories")]
    public int? Calories { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTimeOffset CreatedDate { get; set; }

    [JsonPropertyName("lastModifiedDate")]
    public DateTimeOffset LastModifiedDate { get; set; }

    [JsonPropertyName("components")]
    public List<ComponentDetailDto> Components { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<CategoryDto> Categories { get; set; } = new();
}

public class RecipeSummaryDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("cookingMinutes")]
    public double? CookingMinutes { get; set; }

    [JsonPropertyName("servings")]
    public int? Servings { get; set; }

    [JsonPropertyName("calories")]
    public int? Calories { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }
}
