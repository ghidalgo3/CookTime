using CookTime.Models.Contracts;

namespace CookTime.Services;

// Owns the recipe recommendation algorithm. The database supplies primitives
// (candidate ingredient sets, favorites, cook history, summaries) via CookTimeDB;
// all scoring, the minimum-overlap gate, weighting, and ranking happen here so the
// policy is easy to read, unit-test, and change without a migration.
public class RecommendationService(CookTimeDB db)
{
    // A recipe must share at least this many ingredients with the source to be
    // recommended at all. Favorite and novelty only re-rank within that set.
    public const int MinSharedIngredients = 2;

    // Weights sum to 1.0. Ingredient similarity dominates; favorite and novelty
    // nudge the ranking for signed-in users.
    public const double SimilarityWeight = 0.70;
    public const double FavoriteWeight = 0.18;
    public const double NoveltyWeight = 0.12;

    public async Task<List<RecipeRecommendationDto>?> GetRecommendationsAsync(Guid sourceRecipeId, Guid? userId, int limit)
    {
        var source = await db.GetRecipeByIdAsync(sourceRecipeId);
        if (source == null)
        {
            return null;
        }

        var sourceIngredients = source.RecipeComponents
            .SelectMany(c => c.Ingredients)
            .Select(i => i.Ingredient?.Id)
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .ToHashSet();

        var candidates = await db.GetCandidateIngredientSetsAsync(sourceRecipeId);

        HashSet<Guid> favorites = [];
        Dictionary<Guid, DateOnly> lastCooked = [];
        if (userId.HasValue)
        {
            favorites = await db.GetFavoriteRecipeIdsAsync(userId.Value);
            lastCooked = await db.GetLastCookedDatesAsync(userId.Value);
        }

        var ranked = ScoreCandidates(
            sourceIngredients,
            candidates,
            favorites,
            lastCooked,
            personalized: userId.HasValue,
            today: DateOnly.FromDateTime(DateTime.UtcNow.Date),
            limit: limit);

        if (ranked.Count == 0)
        {
            return [];
        }

        var summaries = await db.GetRecipeSummariesAsync(ranked.Select(r => r.RecipeId).ToList());

        var recommendations = new List<RecipeRecommendationDto>();
        foreach (var scored in ranked)
        {
            if (!summaries.TryGetValue(scored.RecipeId, out var summary))
            {
                continue;
            }

            recommendations.Add(new RecipeRecommendationDto
            {
                Recipe = summary,
                Score = scored.Score,
                ScoreBreakdown = scored.Breakdown,
                Reasons = scored.Reasons
            });
        }

        return recommendations;
    }

    // Pure scoring: no database access, so the recommendation policy is directly
    // unit-testable. Applies the minimum-overlap gate, computes Jaccard similarity,
    // layers favorite/novelty boosts for signed-in users, and ranks.
    public static List<ScoredRecipe> ScoreCandidates(
        IReadOnlySet<Guid> sourceIngredients,
        IReadOnlyList<CandidateIngredientSet> candidates,
        IReadOnlySet<Guid> favoriteRecipeIds,
        IReadOnlyDictionary<Guid, DateOnly> lastCookedByRecipe,
        bool personalized,
        DateOnly today,
        int limit)
    {
        var scored = new List<ScoredRecipe>();

        foreach (var candidate in candidates)
        {
            var candidateIngredients = candidate.IngredientIds.ToHashSet();
            var intersection = candidateIngredients.Count(sourceIngredients.Contains);

            // The hard gate: too little ingredient overlap, never recommend.
            if (intersection < MinSharedIngredients)
            {
                continue;
            }

            var union = sourceIngredients.Count + candidateIngredients.Count - intersection;
            var jaccard = union == 0 ? 0.0 : (double)intersection / union;

            var reasons = new List<string> { "Similar ingredients" };

            double favorite = 0.0;
            double novelty = 0.0;
            if (personalized)
            {
                favorite = favoriteRecipeIds.Contains(candidate.RecipeId) ? 1.0 : 0.0;
                novelty = NoveltyScore(candidate.RecipeId, lastCookedByRecipe, today);

                if (favorite > 0)
                {
                    reasons.Add("Favorite");
                }
                if (novelty > 0)
                {
                    reasons.Add("Not cooked recently");
                }
            }

            var similarityContribution = jaccard * SimilarityWeight;
            var favoriteContribution = favorite * FavoriteWeight;
            var noveltyContribution = novelty * NoveltyWeight;
            var total = personalized
                ? similarityContribution + favoriteContribution + noveltyContribution
                : jaccard;

            scored.Add(new ScoredRecipe(
                candidate.RecipeId,
                total,
                intersection,
                new RecommendationScoreBreakdownDto
                {
                    IngredientSimilarity = personalized ? similarityContribution : jaccard,
                    FavoritedByUser = favoriteContribution,
                    Novelty = noveltyContribution,
                    DietMatch = 0
                },
                reasons));
        }

        return scored
            .OrderByDescending(s => s.Score)
            .ThenByDescending(s => s.SharedIngredients)
            .ThenBy(s => s.RecipeId)
            .Take(Math.Max(0, limit))
            .ToList();
    }

    private static double NoveltyScore(Guid recipeId, IReadOnlyDictionary<Guid, DateOnly> lastCookedByRecipe, DateOnly today)
    {
        if (!lastCookedByRecipe.TryGetValue(recipeId, out var lastCooked))
        {
            return 1.0;
        }

        var daysSince = today.DayNumber - lastCooked.DayNumber;
        if (daysSince <= 7)
        {
            return 0.0;
        }
        if (daysSince <= 30)
        {
            return 0.5;
        }
        return 1.0;
    }

    public sealed record ScoredRecipe(
        Guid RecipeId,
        double Score,
        int SharedIngredients,
        RecommendationScoreBreakdownDto Breakdown,
        List<string> Reasons);
}
