using System.Text.Json.Serialization;

namespace BabeAlgorithms.Models.Contracts;

public class RecipeListDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("creationDate")]
    public DateTimeOffset CreationDate { get; set; }

    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; set; }

    [JsonPropertyName("recipeCount")]
    public int RecipeCount { get; set; }
}

public class RecipeListCreateDto
{
    [JsonPropertyName("ownerId")]
    public Guid OwnerId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; set; }
}

public class RecipeListWithRecipesDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("creationDate")]
    public DateTimeOffset CreationDate { get; set; }

    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; set; }

    [JsonPropertyName("ownerId")]
    public Guid OwnerId { get; set; }

    [JsonPropertyName("recipes")]
    public List<RecipeListItemDto> Recipes { get; set; } = new();
}

public class RecipeListItemDto
{
    [JsonPropertyName("recipeId")]
    public Guid RecipeId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("quantity")]
    public double Quantity { get; set; }

    [JsonPropertyName("cookingMinutes")]
    public double? CookingMinutes { get; set; }

    [JsonPropertyName("servings")]
    public int? Servings { get; set; }
}
