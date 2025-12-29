using System.Text.Json.Serialization;

namespace BabeAlgorithms.Models.Contracts;

public class RecipeDetailDto
{
    [JsonPropertyName("recipeId")]
    public int RecipeId { get; set; }

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

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTimeOffset CreatedDate { get; set; }

    [JsonPropertyName("lastModifiedDate")]
    public DateTimeOffset LastModifiedDate { get; set; }

    [JsonPropertyName("components")]
    public List<ComponentDetailDto> Components { get; set; } = new();

    [JsonPropertyName("steps")]
    public List<RecipeStepDto> Steps { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<CategoryDto> Categories { get; set; } = new();
}

public class RecipeSummaryDto
{
    [JsonPropertyName("recipeId")]
    public int RecipeId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("prepTime")]
    public TimeSpan? PrepTime { get; set; }

    [JsonPropertyName("cookTime")]
    public TimeSpan? CookTime { get; set; }

    [JsonPropertyName("servings")]
    public int? Servings { get; set; }

    [JsonPropertyName("calories")]
    public int? Calories { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }
}
