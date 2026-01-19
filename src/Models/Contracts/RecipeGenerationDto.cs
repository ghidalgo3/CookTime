using System.Text.Json.Serialization;

namespace BabeAlgorithms.Models.Contracts;

/// <summary>
/// Result of AI-powered recipe generation, including the draft recipe and ingredient match metadata.
/// </summary>
public class RecipeGenerationResultDto
{
    /// <summary>
    /// The generated recipe data, ready for user review and editing.
    /// IngredientId will be populated for high-confidence matches, null for unmatched ingredients.
    /// </summary>
    [JsonPropertyName("recipe")]
    public RecipeCreateDto Recipe { get; set; } = null!;

    /// <summary>
    /// Match metadata for each ingredient, in the same order as they appear in the recipe components.
    /// </summary>
    [JsonPropertyName("ingredientMatches")]
    public List<IngredientMatchDto> IngredientMatches { get; set; } = [];
}

/// <summary>
/// Metadata about how an AI-extracted ingredient was matched to the database.
/// </summary>
public class IngredientMatchDto
{
    /// <summary>
    /// The original ingredient text extracted by the AI.
    /// </summary>
    [JsonPropertyName("originalText")]
    public string OriginalText { get; set; } = null!;

    /// <summary>
    /// The matched ingredient ID, or null if no confident match was found.
    /// </summary>
    [JsonPropertyName("matchedIngredientId")]
    public Guid? MatchedIngredientId { get; set; }

    /// <summary>
    /// The name of the matched ingredient, or null if no match.
    /// </summary>
    [JsonPropertyName("matchedIngredientName")]
    public string? MatchedIngredientName { get; set; }

    /// <summary>
    /// Confidence score of the match (0.0 to 1.0). Null if no match attempted.
    /// </summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    /// <summary>
    /// Alternative candidate matches for the user to choose from.
    /// </summary>
    [JsonPropertyName("candidates")]
    public List<IngredientCandidateDto> Candidates { get; set; } = [];
}

/// <summary>
/// A candidate ingredient match with its confidence score.
/// </summary>
public class IngredientCandidateDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}

/// <summary>
/// Request body for text-to-recipe generation.
/// </summary>
public class GenerateFromTextRequest
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = null!;
}

/// <summary>
/// Internal DTO for deserializing AI-generated recipe structure before mapping to RecipeCreateDto.
/// </summary>
internal class AIRecipeResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("servings")]
    public int? Servings { get; set; }

    [JsonPropertyName("prepMinutes")]
    public double? PrepMinutes { get; set; }

    [JsonPropertyName("cookingMinutes")]
    public double? CookingMinutes { get; set; }

    [JsonPropertyName("components")]
    public List<AIComponentResponse> Components { get; set; } = [];
}

internal class AIComponentResponse
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("ingredients")]
    public List<AIIngredientResponse> Ingredients { get; set; } = [];

    [JsonPropertyName("steps")]
    public List<string> Steps { get; set; } = [];
}

internal class AIIngredientResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("quantity")]
    public double Quantity { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = null!;

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Extended autosuggest DTO that includes confidence score from similarity matching.
/// </summary>
public class IngredientMatchResultDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}
