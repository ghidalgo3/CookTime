using System.Text.Json.Serialization;

namespace BabeAlgorithms.Models.Contracts;

public class RecipeListDto
{
    [JsonPropertyName("listId")]
    public int ListId { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("state")]
    public string State { get; set; } = null!;

    [JsonPropertyName("createdDate")]
    public DateTimeOffset CreatedDate { get; set; }
}

public class RecipeListCreateDto
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("state")]
    public string State { get; set; } = "active";
}

public class RecipeListWithRecipesDto : RecipeListDto
{
    [JsonPropertyName("recipes")]
    public List<RecipeSummaryDto> Recipes { get; set; } = new();
}

public class RecipeRequirementDto
{
    [JsonPropertyName("requirementId")]
    public int RequirementId { get; set; }

    [JsonPropertyName("recipeId")]
    public int RecipeId { get; set; }

    [JsonPropertyName("servings")]
    public int Servings { get; set; }
}
