using System.Text.Json.Serialization;

namespace BabeAlgorithms.Models.Contracts;

public class RecipeCreateDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("ownerId")]
    public Guid OwnerId { get; set; }

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

    [JsonPropertyName("components")]
    public List<ComponentCreateDto> Components { get; set; } = new();

    [JsonPropertyName("categoryIds")]
    public List<Guid> CategoryIds { get; set; } = new();
}

public class RecipeUpdateDto : RecipeCreateDto
{
}

public class ComponentCreateDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("steps")]
    public List<string> Steps { get; set; } = new();

    [JsonPropertyName("ingredients")]
    public List<IngredientRequirementCreateDto> Ingredients { get; set; } = new();
}

public class IngredientRequirementCreateDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("ingredient")]
    public IngredientRefDto Ingredient { get; set; } = null!;

    [JsonPropertyName("quantity")]
    public double Quantity { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public record RecipeCreateRequest(string Name);