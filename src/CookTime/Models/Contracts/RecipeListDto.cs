using System.Text.Json.Serialization;

namespace CookTime.Models.Contracts;

public class RecipeListDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = null!;

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

public class RecipeListItemDto
{
    [JsonPropertyName("recipe")]
    public RecipeSummaryDto Recipe { get; set; } = null!;

    [JsonPropertyName("quantity")]
    public double Quantity { get; set; }
}

public class RecipeListWithRecipesDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = null!;

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

    [JsonPropertyName("selectedIngredients")]
    public List<Guid> SelectedIngredients { get; set; } = new();
}

/// <summary>
/// Simple ingredient representation for aggregated ingredient lists.
/// Matches the JSON structure returned by get_list_aggregated_ingredients SQL function.
/// </summary>
public class SimpleIngredientDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("isNew")]
    public bool IsNew { get; set; }

    [JsonPropertyName("densityKgPerL")]
    public double DensityKgPerL { get; set; }
}

public class AggregatedIngredientDto
{
    [JsonPropertyName("ingredient")]
    public SimpleIngredientDto Ingredient { get; set; } = null!;

    [JsonPropertyName("quantity")]
    public double Quantity { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = null!;

    [JsonPropertyName("selected")]
    public bool Selected { get; set; }
}

public class RecipeListUpdateDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("isPublic")]
    public bool? IsPublic { get; set; }
}
