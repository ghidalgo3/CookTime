using BabeAlgorithms.Models.Contracts;
using BabeAlgorithms.Services;

namespace BabeAlgorithms.Routes;

public static class ListRoutes
{
    public static RouteGroupBuilder MapListRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/lists", async (HttpContext context, CookTimeDB cooktime) =>
        {
            var userId = (Guid)context.Items["UserId"]!;
            var lists = await cooktime.GetRecipeListsAsync(userId);
            return Results.Ok(lists);
        });

        // Create an empty list if it doesn't exist
        group.MapPost("/lists/{listName}", async (HttpContext context, CookTimeDB cooktime, string listName) =>
        {
            var userId = (Guid)context.Items["UserId"]!;
            var lists = await cooktime.GetRecipeListsAsync(userId, filter: listName);

            if (lists.Count > 0)
            {
                // List already exists
                return Results.NoContent();
            }

            // Create the list
            await cooktime.CreateRecipeListAsync(new RecipeListCreateDto
            {
                OwnerId = userId,
                Name = listName,
                Description = $"My {listName} list",
                IsPublic = false
            });

            return Results.Created();
        });

        group.MapGet("/lists/{listName}", async (HttpContext context, CookTimeDB cooktime, string listName) =>
        {
            var userId = (Guid)context.Items["UserId"]!;

            // Try to parse as GUID first for listId lookup
            if (Guid.TryParse(listName, out var listId))
            {
                var listById = await cooktime.GetRecipeListWithRecipesAsync(listId);
                if (listById != null && listById.OwnerId == userId)
                {
                    return Results.Ok(listById);
                }
                return Results.NotFound();
            }

            // Otherwise filter by name
            var lists = await cooktime.GetRecipeListsAsync(userId, filter: listName);
            if (lists.Count == 0)
            {
                return Results.NotFound();
            }

            var listWithRecipes = await cooktime.GetRecipeListWithRecipesAsync(lists[0].Id);
            return Results.Ok(listWithRecipes);
        });

        group.MapPut("/lists/{listName}/{recipeId:guid}", async (HttpContext context, CookTimeDB cooktime, string listName, Guid recipeId, double? quantity) =>
        {
            var userId = (Guid)context.Items["UserId"]!;
            var lists = await cooktime.GetRecipeListsAsync(userId, filter: listName);

            Guid listId;
            if (lists.Count == 0)
            {
                // Create the list if it doesn't exist
                listId = await cooktime.CreateRecipeListAsync(new RecipeListCreateDto
                {
                    OwnerId = userId,
                    Name = listName,
                    Description = $"My {listName} list",
                    IsPublic = false
                });
            }
            else
            {
                listId = lists[0].Id;
            }

            await cooktime.AddRecipeToListAsync(listId, recipeId, quantity ?? 1.0);
            return Results.Ok();
        });

        // Update recipe quantity in list
        group.MapPatch("/lists/{listName}/{recipeId:guid}", async (HttpContext context, CookTimeDB cooktime, string listName, Guid recipeId, double quantity) =>
        {
            var userId = (Guid)context.Items["UserId"]!;
            var lists = await cooktime.GetRecipeListsAsync(userId, filter: listName);

            if (lists.Count == 0)
            {
                return Results.NotFound();
            }

            await cooktime.UpdateRecipeQuantityInListAsync(lists[0].Id, recipeId, quantity);
            return Results.Ok();
        });

        group.MapDelete("/lists/{listName}/{recipeId:guid}", async (HttpContext context, CookTimeDB cooktime, string listName, Guid recipeId) =>
        {
            var userId = (Guid)context.Items["UserId"]!;
            var lists = await cooktime.GetRecipeListsAsync(userId, filter: listName);

            if (lists.Count == 0)
            {
                return Results.NotFound();
            }

            await cooktime.RemoveRecipeFromListAsync(lists[0].Id, recipeId);
            return Results.Ok();
        });

        // Get aggregated ingredients for a list
        group.MapGet("/lists/{listName}/ingredients", async (HttpContext context, CookTimeDB cooktime, string listName) =>
        {
            var userId = (Guid)context.Items["UserId"]!;
            var lists = await cooktime.GetRecipeListsAsync(userId, filter: listName);

            if (lists.Count == 0)
            {
                return Results.NotFound();
            }

            var ingredients = await cooktime.GetListAggregatedIngredientsAsync(lists[0].Id);
            return Results.Ok(ingredients);
        });

        // Toggle selected state of an ingredient in a list
        group.MapPut("/lists/{listName}/ingredients/{ingredientId:guid}", async (HttpContext context, CookTimeDB cooktime, string listName, Guid ingredientId) =>
        {
            var userId = (Guid)context.Items["UserId"]!;
            var lists = await cooktime.GetRecipeListsAsync(userId, filter: listName);

            if (lists.Count == 0)
            {
                return Results.NotFound();
            }

            var isNowSelected = await cooktime.ToggleSelectedIngredientAsync(lists[0].Id, ingredientId);
            return Results.Ok(new { selected = isNowSelected });
        });

        // Clear all selected ingredients in a list
        group.MapDelete("/lists/{listName}/ingredients", async (HttpContext context, CookTimeDB cooktime, string listName) =>
        {
            var userId = (Guid)context.Items["UserId"]!;
            var lists = await cooktime.GetRecipeListsAsync(userId, filter: listName);

            if (lists.Count == 0)
            {
                return Results.NotFound();
            }

            var clearedCount = await cooktime.ClearSelectedIngredientsAsync(lists[0].Id);
            return Results.Ok(new { clearedCount });
        });

        return group;
    }
}
