using CookTime.Models.Contracts;
using CookTime.Services;

namespace CookTime.Test;

// Pure unit tests for the recommendation scoring policy. These need no database:
// moving the algorithm into the application is exactly what makes this possible.
[TestClass]
public class TestRecommendationService
{
    private static readonly DateOnly Today = new(2026, 6, 14);

    private static CandidateIngredientSet Candidate(Guid id, params Guid[] ingredients) =>
        new() { RecipeId = id, IngredientIds = [.. ingredients] };

    [TestMethod]
    public void ScoreCandidates_ExcludesRecipesBelowMinimumOverlap()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();
        var source = new HashSet<Guid> { a, b, c };

        var twoOverlap = Guid.NewGuid();
        var oneOverlap = Guid.NewGuid();
        var candidates = new List<CandidateIngredientSet>
        {
            Candidate(twoOverlap, a, b),       // 2 shared -> included
            Candidate(oneOverlap, a, Guid.NewGuid()), // 1 shared -> excluded
        };

        var result = RecommendationService.ScoreCandidates(
            source, candidates, favoriteRecipeIds: new HashSet<Guid>(), lastCookedByRecipe: new Dictionary<Guid, DateOnly>(),
            personalized: false, today: Today, limit: 10);

        Assert.HasCount(1, result);
        Assert.AreEqual(twoOverlap, result[0].RecipeId);
    }

    [TestMethod]
    public void ScoreCandidates_AnonymousScoreIsRawJaccard()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var source = new HashSet<Guid> { a, b };
        var candidateId = Guid.NewGuid();
        var candidates = new List<CandidateIngredientSet> { Candidate(candidateId, a, b) };

        var result = RecommendationService.ScoreCandidates(
            source, candidates, favoriteRecipeIds: new HashSet<Guid>(), lastCookedByRecipe: new Dictionary<Guid, DateOnly>(),
            personalized: false, today: Today, limit: 10);

        // Identical ingredient sets -> Jaccard 1.0; no personalization weighting.
        Assert.AreEqual(1.0, result[0].Score, 0.001);
        Assert.AreEqual(1.0, result[0].Breakdown.IngredientSimilarity, 0.001);
        Assert.AreEqual(0, result[0].Breakdown.FavoritedByUser, 0.001);
        Assert.AreEqual(0, result[0].Breakdown.Novelty, 0.001);
    }

    [TestMethod]
    public void ScoreCandidates_PersonalizedAppliesWeightsAndRanksNoveltyHigher()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var source = new HashSet<Guid> { a, b };

        var neverCooked = Guid.NewGuid();
        var recentlyCooked = Guid.NewGuid();
        var candidates = new List<CandidateIngredientSet>
        {
            Candidate(neverCooked, a, b),
            Candidate(recentlyCooked, a, b),
        };

        var favorites = new HashSet<Guid> { neverCooked, recentlyCooked };
        var lastCooked = new Dictionary<Guid, DateOnly> { [recentlyCooked] = Today.AddDays(-1) };

        var result = RecommendationService.ScoreCandidates(
            source, candidates, favorites, lastCooked,
            personalized: true, today: Today, limit: 10);

        var never = result.Single(r => r.RecipeId == neverCooked);
        var recent = result.Single(r => r.RecipeId == recentlyCooked);

        // Jaccard 1.0 * 0.70 + favorite 0.18 + novelty 0.12 = 1.0
        Assert.AreEqual(0.70, never.Breakdown.IngredientSimilarity, 0.001);
        Assert.AreEqual(0.18, never.Breakdown.FavoritedByUser, 0.001);
        Assert.AreEqual(0.12, never.Breakdown.Novelty, 0.001);
        Assert.AreEqual(1.0, never.Score, 0.001);

        // Cooked yesterday -> novelty 0, so it ranks below the never-cooked recipe.
        Assert.AreEqual(0, recent.Breakdown.Novelty, 0.001);
        Assert.AreEqual(0.88, recent.Score, 0.001);
        Assert.AreEqual(neverCooked, result[0].RecipeId);
    }

    [TestMethod]
    public void ScoreCandidates_RespectsLimit()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();
        var source = new HashSet<Guid> { a, b, c };
        var candidates = new List<CandidateIngredientSet>
        {
            Candidate(Guid.NewGuid(), a, b, c),
            Candidate(Guid.NewGuid(), a, b),
            Candidate(Guid.NewGuid(), a, b),
        };

        var result = RecommendationService.ScoreCandidates(
            source, candidates, favoriteRecipeIds: new HashSet<Guid>(), lastCookedByRecipe: new Dictionary<Guid, DateOnly>(),
            personalized: false, today: Today, limit: 2);

        Assert.HasCount(2, result);
    }
}
