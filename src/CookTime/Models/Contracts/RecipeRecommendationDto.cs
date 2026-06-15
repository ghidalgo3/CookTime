namespace CookTime.Models.Contracts;

public class CookHistoryEventDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("recipeId")]
    public Guid RecipeId { get; set; }

    [JsonPropertyName("cookedAt")]
    public DateOnly CookedAt { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; }
}

public class CookHistoryEventWithRecipeDto : CookHistoryEventDto
{
    [JsonPropertyName("recipe")]
    public RecipeSummaryDto Recipe { get; set; } = null!;
}

public record CookHistoryUpsertRequest(
    [property: JsonPropertyName("cookedAt")] DateOnly? CookedAt
);

public class RecommendationScoreBreakdownDto
{
    [JsonPropertyName("ingredientSimilarity")]
    public double IngredientSimilarity { get; set; }

    [JsonPropertyName("favoritedByUser")]
    public double FavoritedByUser { get; set; }

    [JsonPropertyName("novelty")]
    public double Novelty { get; set; }

    [JsonPropertyName("dietMatch")]
    public double DietMatch { get; set; }
}

public class RecipeRecommendationDto
{
    [JsonPropertyName("recipe")]
    public RecipeSummaryDto Recipe { get; set; } = null!;

    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("scoreBreakdown")]
    public RecommendationScoreBreakdownDto ScoreBreakdown { get; set; } = new();

    [JsonPropertyName("reasons")]
    public List<string> Reasons { get; set; } = new();
}

// A candidate recipe and its set of ingredient IDs, as returned by the
// cooktime.get_candidate_ingredient_sets primitive. The recommendation
// algorithm consumes these to compute ingredient similarity.
public class CandidateIngredientSet
{
    [JsonPropertyName("recipeId")]
    public Guid RecipeId { get; set; }

    [JsonPropertyName("ingredientIds")]
    public List<Guid> IngredientIds { get; set; } = new();
}
