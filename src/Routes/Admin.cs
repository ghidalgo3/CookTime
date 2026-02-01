using BabeAlgorithms.Models.Contracts;
using BabeAlgorithms.Services;
using Npgsql;

namespace BabeAlgorithms.Routes;

public static class AdminRoutes
{
    public static RouteGroupBuilder MapAdminRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/ingredient/internalUpdate", async (CookTimeDB cooktime) =>
        {
            var ingredients = await cooktime.GetIngredientsForAdminAsync();
            return Results.Ok(ingredients);
        });

        group.MapGet("/ingredient/unified", async (CookTimeDB cooktime) =>
        {
            var ingredients = await cooktime.GetIngredientsUnifiedAsync();
            return Results.Ok(ingredients);
        });

        group.MapPost("/ingredient/internalupdate", async (CookTimeDB cooktime, IngredientInternalUpdateDto update) =>
        {
            var result = await cooktime.UpdateIngredientInternalAsync(update);
            if (result == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(result);
        });

        group.MapGet("/ingredient/normalized", async (CookTimeDB cooktime) =>
        {
            var ingredients = await cooktime.GetNormalizedIngredientsAsync();
            return Results.Ok(ingredients);
        });

        group.MapPost("/ingredient/replace", async (CookTimeDB cooktime, MergeIngredientsDto request) =>
        {
            // Validate that the two ingredients are different
            if (request.FromIngredientId == request.ToIngredientId)
            {
                return Results.BadRequest("Cannot merge an ingredient with itself");
            }

            try
            {
                await cooktime.MergeIngredientsAsync(request.FromIngredientId, request.ToIngredientId);
                return Results.Ok();
            }
            catch (PostgresException ex) when (ex.SqlState == "P0001") // Custom exception from stored procedure
            {
                return Results.BadRequest(ex.MessageText);
            }
        });

        group.MapDelete("/ingredient/{ingredientId:guid}", async (CookTimeDB cooktime, Guid ingredientId) =>
        {
            try
            {
                await cooktime.DeleteIngredientAsync(ingredientId);
                return Results.NoContent();
            }
            catch (PostgresException ex) when (ex.SqlState == "P0001")
            {
                return Results.BadRequest(ex.MessageText);
            }
        });

        group.MapGet("/nutrition/search", async (CookTimeDB cooktime, string? query, string? dataset) =>
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Results.Ok(Array.Empty<NutritionFactsSearchDto>());
            }
            var results = await cooktime.SearchNutritionFactsAsync(query, dataset);
            return Results.Ok(results);
        });

        return group;
    }
}