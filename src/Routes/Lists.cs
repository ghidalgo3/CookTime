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

        group.MapPut("/lists/{listName}/{recipeId:guid}", async (HttpContext context, CookTimeDB cooktime, string listName, Guid recipeId) =>
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

            await cooktime.AddRecipeToListAsync(listId, recipeId, 1);
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

        return group;
    }
}
