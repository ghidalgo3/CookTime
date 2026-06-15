using CookTime.Models.Contracts;
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
    public async Task Recommendations_ExcludeSourceAndPreferIngredientOverlap()
    {
        var sharedIngredientId = await CreateIngredient("Shared Recommendation Ingredient");
        var unrelatedIngredientId = await CreateIngredient("Unrelated Recommendation Ingredient");
        var sourceRecipeId = await CreateRecipe("Source Recipe", TestUserId, sharedIngredientId);
        var similarRecipeId = await CreateRecipe("Similar Recipe", TestUserId, sharedIngredientId);
        var unrelatedRecipeId = await CreateRecipe("Unrelated Recipe", TestUserId, unrelatedIngredientId);

        var recommendations = await Db.GetRecipeRecommendationsAsync(sourceRecipeId, userId: null, limit: 10);

        Assert.IsFalse(recommendations.Any(r => r.Recipe.Id == sourceRecipeId));
        Assert.IsTrue(recommendations.Any(r => r.Recipe.Id == similarRecipeId));
        Assert.IsFalse(recommendations.Any(r => r.Recipe.Id == unrelatedRecipeId));
        Assert.AreEqual(similarRecipeId, recommendations[0].Recipe.Id);
        Assert.IsGreaterThan(recommendations[0].ScoreBreakdown.IngredientSimilarity, 0);
    }

    [TestMethod]
    public async Task Recommendations_IncludeFavoriteOwnedAndNoveltyBoosts()
    {
        var sharedIngredientId = await CreateIngredient("Personalized Recommendation Ingredient");
        var sourceRecipeId = await CreateRecipe("Personalized Source", TestUserId, sharedIngredientId);
        var neverCookedRecipeId = await CreateRecipe("Never Cooked Favorite", TestUserId, sharedIngredientId);
        var recentlyCookedRecipeId = await CreateRecipe("Recently Cooked Favorite", TestUserId, sharedIngredientId);
        var favoritesListId = await Db.CreateRecipeListAsync(new RecipeListCreateDto
        {
            OwnerId = TestUserId,
            Name = "Favorites"
        });

        await Db.AddRecipeToListAsync(favoritesListId, neverCookedRecipeId, 1);
        await Db.AddRecipeToListAsync(favoritesListId, recentlyCookedRecipeId, 1);
        await Db.CreateCookHistoryEventAsync(TestUserId, recentlyCookedRecipeId, DateOnly.FromDateTime(DateTime.Today));

        var recommendations = await Db.GetRecipeRecommendationsAsync(sourceRecipeId, TestUserId, limit: 10);
        var neverCooked = recommendations.Single(r => r.Recipe.Id == neverCookedRecipeId);
        var recentlyCooked = recommendations.Single(r => r.Recipe.Id == recentlyCookedRecipeId);

        Assert.AreEqual(0.15, neverCooked.ScoreBreakdown.OwnedByUser, 0.001);
        Assert.AreEqual(0.15, neverCooked.ScoreBreakdown.FavoritedByUser, 0.001);
        Assert.AreEqual(0.10, neverCooked.ScoreBreakdown.Novelty, 0.001);
        Assert.AreEqual(0, recentlyCooked.ScoreBreakdown.Novelty, 0.001);
        Assert.IsGreaterThan(neverCooked.Score, recentlyCooked.Score);
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
