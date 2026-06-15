using CookTime.Models.Contracts;
using CookTime.Services;
using Npgsql;

namespace CookTime.Test;

[TestClass]
public class TestCookHistoryAndRecommendations : TestBase
{
    private readonly List<Guid> _ingredientIds = [];

    [TestInitialize]
    public async Task TestInitialize()
    {
        await InitializeAsync(nameof(TestCookHistoryAndRecommendations));
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        foreach (var ingredientId in _ingredientIds)
        {
            await DeleteTestIngredientAsync(ingredientId);
        }

        await CleanupAsync();
    }

    [TestMethod]
    public async Task CookHistory_CrudEnforcesOwnership()
    {
        var ingredientId = await CreateIngredient("Cook History Ingredient");
        var recipeId = await CreateRecipe("Cook History Recipe", TestUserId, ingredientId);
        var otherUserId = await CreateTestUserAsync("Cook History Other");

        try
        {
            var cookEvent = await Db.CreateCookHistoryEventAsync(TestUserId, recipeId, new DateOnly(2026, 1, 1));

            Assert.AreNotEqual(Guid.Empty, cookEvent.Id);
            Assert.AreEqual(recipeId, cookEvent.RecipeId);
            Assert.AreEqual(new DateOnly(2026, 1, 1), cookEvent.CookedAt);

            var history = await Db.GetCookHistoryAsync(TestUserId, recipeId);
            Assert.HasCount(1, history);

            await Assert.ThrowsExactlyAsync<PostgresException>(() =>
                Db.UpdateCookHistoryEventAsync(otherUserId, cookEvent.Id, new DateOnly(2026, 1, 2)));

            var updated = await Db.UpdateCookHistoryEventAsync(TestUserId, cookEvent.Id, new DateOnly(2026, 1, 3));
            Assert.IsNotNull(updated);
            Assert.AreEqual(new DateOnly(2026, 1, 3), updated.CookedAt);

            await Assert.ThrowsExactlyAsync<PostgresException>(() =>
                Db.DeleteCookHistoryEventAsync(otherUserId, cookEvent.Id));

            var deleted = await Db.DeleteCookHistoryEventAsync(TestUserId, cookEvent.Id);
            Assert.IsTrue(deleted);
            Assert.IsEmpty(await Db.GetCookHistoryAsync(TestUserId, recipeId));
        }
        finally
        {
            await DeleteTestUserAsync(otherUserId);
        }
    }

    [TestMethod]
    public async Task CookHistory_DeleteRecipeCascadesCookEvents()
    {
        var ingredientId = await CreateIngredient("Cascade Ingredient");
        var recipeId = await CreateRecipe("Cascade Recipe", TestUserId, ingredientId);
        var cookEvent = await Db.CreateCookHistoryEventAsync(TestUserId, recipeId, new DateOnly(2026, 2, 1));

        await Db.DeleteRecipeAsync(recipeId);

        await using var conn = await DataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM cooktime.recipe_cook_events WHERE id = $1", conn);
        cmd.Parameters.AddWithValue(cookEvent.Id);
        var count = (long)(await cmd.ExecuteScalarAsync())!;

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task Recommendations_ExcludeSourceAndRequireMinimumOverlap()
    {
        var sharedA = await CreateIngredient("Shared Recommendation Ingredient A");
        var sharedB = await CreateIngredient("Shared Recommendation Ingredient B");
        var unrelatedIngredientId = await CreateIngredient("Unrelated Recommendation Ingredient");
        var sourceRecipeId = await CreateRecipe("Source Recipe", TestUserId, sharedA, sharedB);
        // Two shared ingredients clears the minimum-overlap gate.
        var similarRecipeId = await CreateRecipe("Similar Recipe", TestUserId, sharedA, sharedB);
        // Only one shared ingredient: below the gate, must not be recommended.
        var oneOverlapRecipeId = await CreateRecipe("One Overlap Recipe", TestUserId, sharedA, unrelatedIngredientId);
        // No shared ingredients: never recommended.
        var unrelatedRecipeId = await CreateRecipe("Unrelated Recipe", TestUserId, unrelatedIngredientId);

        var recommendations = await new RecommendationService(Db).GetRecommendationsAsync(sourceRecipeId, userId: null, limit: 10);
        Assert.IsNotNull(recommendations);

        Assert.IsFalse(recommendations.Any(r => r.Recipe.Id == sourceRecipeId));
        Assert.IsTrue(recommendations.Any(r => r.Recipe.Id == similarRecipeId));
        Assert.IsFalse(recommendations.Any(r => r.Recipe.Id == oneOverlapRecipeId));
        Assert.IsFalse(recommendations.Any(r => r.Recipe.Id == unrelatedRecipeId));
        Assert.AreEqual(similarRecipeId, recommendations[0].Recipe.Id);
        Assert.IsGreaterThan(0, recommendations[0].ScoreBreakdown.IngredientSimilarity);
    }

    [TestMethod]
    public async Task Recommendations_IncludeFavoriteAndNoveltyBoosts()
    {
        var sharedA = await CreateIngredient("Personalized Recommendation Ingredient A");
        var sharedB = await CreateIngredient("Personalized Recommendation Ingredient B");
        var sourceRecipeId = await CreateRecipe("Personalized Source", TestUserId, sharedA, sharedB);
        var neverCookedRecipeId = await CreateRecipe("Never Cooked Favorite", TestUserId, sharedA, sharedB);
        var recentlyCookedRecipeId = await CreateRecipe("Recently Cooked Favorite", TestUserId, sharedA, sharedB);
        var favoritesListId = await Db.CreateRecipeListAsync(new RecipeListCreateDto
        {
            OwnerId = TestUserId,
            Name = "Favorites"
        });

        await Db.AddRecipeToListAsync(favoritesListId, neverCookedRecipeId, 1);
        await Db.AddRecipeToListAsync(favoritesListId, recentlyCookedRecipeId, 1);
        await Db.CreateCookHistoryEventAsync(TestUserId, recentlyCookedRecipeId, DateOnly.FromDateTime(DateTime.Today));

        var recommendations = await new RecommendationService(Db).GetRecommendationsAsync(sourceRecipeId, TestUserId, limit: 10);
        Assert.IsNotNull(recommendations);
        var neverCooked = recommendations.Single(r => r.Recipe.Id == neverCookedRecipeId);
        var recentlyCooked = recommendations.Single(r => r.Recipe.Id == recentlyCookedRecipeId);

        Assert.AreEqual(0.18, neverCooked.ScoreBreakdown.FavoritedByUser, 0.001);
        Assert.AreEqual(0.12, neverCooked.ScoreBreakdown.Novelty, 0.001);
        Assert.AreEqual(0, recentlyCooked.ScoreBreakdown.Novelty, 0.001);
        // The only difference is novelty, so the never-cooked recipe ranks higher.
        Assert.IsGreaterThan(recentlyCooked.Score, neverCooked.Score);
    }

    private async Task<Guid> CreateIngredient(string name)
    {
        var ingredientId = await CreateTestIngredientAsync($"{name} {Guid.NewGuid()}");
        _ingredientIds.Add(ingredientId);
        return ingredientId;
    }

    private Task<Guid> CreateRecipe(string name, Guid ownerId, params Guid[] ingredientIds)
    {
        var ingredients = ingredientIds.Select((ingredientId, index) => new IngredientRequirementCreateDto
        {
            Ingredient = new IngredientRefDto
            {
                Id = ingredientId,
                Name = $"Ingredient {index}"
            },
            Quantity = 1,
            Unit = "cup",
            Position = index
        }).ToList();

        return Db.CreateRecipeAsync(new RecipeCreateDto
        {
            OwnerId = ownerId,
            Name = $"{name} {Guid.NewGuid()}",
            Description = "Recommendation test recipe",
            Servings = 2,
            Components =
            [
                new ComponentCreateDto
                {
                    Name = "Main",
                    Position = 1,
                    Ingredients = ingredients
                }
            ]
        });
    }
}
