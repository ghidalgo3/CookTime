using babe_algorithms.Services;
using BabeAlgorithms.Models.Contracts;
using BabeAlgorithms.Services;

namespace BabeAlgorithms.Routes;

public static class RecipeGenerationRoutes
{
    private const int MaxImages = 3;
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5MB per image
    private static readonly HashSet<string> AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp"];

    public static RouteGroupBuilder MapRecipeGenerationRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/recipe/generate-from-image", GenerateFromImageAsync)
            .DisableAntiforgery(); // Required for multipart form uploads

        group.MapPost("/recipe/generate-from-text", GenerateFromTextAsync);

        return group;
    }

    private static async Task<IResult> GenerateFromImageAsync(
        HttpContext context,
        AIRecipeService aiRecipeService,
        CookTimeDB cooktime,
        ILogger<AIRecipeService> logger)
    {
        var userId = (Guid)context.Items["UserId"]!;

        if (!context.Request.HasFormContentType)
        {
            return Results.BadRequest(new { error = "Request must be multipart/form-data" });
        }

        var form = await context.Request.ReadFormAsync();
        var files = form.Files;

        if (files.Count == 0)
        {
            return Results.BadRequest(new { error = "At least one image is required" });
        }

        if (files.Count > MaxImages)
        {
            return Results.BadRequest(new { error = $"Maximum {MaxImages} images allowed" });
        }

        var images = new List<(string Base64Data, string MimeType)>();

        foreach (var file in files)
        {
            // Validate MIME type
            if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return Results.BadRequest(new { error = $"Invalid file type: {file.ContentType}. Allowed types: JPEG, PNG, WebP" });
            }

            // Validate file size
            if (file.Length > MaxImageSizeBytes)
            {
                return Results.BadRequest(new { error = $"File '{file.FileName}' exceeds maximum size of 5MB" });
            }

            // Read file to base64
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var base64 = Convert.ToBase64String(memoryStream.ToArray());
            images.Add((base64, file.ContentType));
        }

        try
        {
            var result = await aiRecipeService.GenerateFromImagesAsync(images, userId);

            // Save the generated recipe to the database
            var recipeId = await cooktime.CreateRecipeAsync(result.Recipe);
            result.Recipe.Id = recipeId;

            logger.LogInformation("Created recipe {RecipeId} from image generation for user {UserId}", recipeId, userId);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating recipe from images");
            return Results.Problem("Failed to generate recipe from images. Please try again.");
        }
    }

    private static async Task<IResult> GenerateFromTextAsync(
        HttpContext context,
        AIRecipeService aiRecipeService,
        CookTimeDB cooktime,
        GenerateFromTextRequest request,
        ILogger<AIRecipeService> logger)
    {
        var userId = (Guid)context.Items["UserId"]!;

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return Results.BadRequest(new { error = "Text is required" });
        }

        // Limit text length to prevent abuse
        if (request.Text.Length > 50000)
        {
            return Results.BadRequest(new { error = "Text exceeds maximum length of 50,000 characters" });
        }

        try
        {
            var result = await aiRecipeService.GenerateFromTextAsync(request.Text, userId);

            // Save the generated recipe to the database
            var recipeId = await cooktime.CreateRecipeAsync(result.Recipe);
            result.Recipe.Id = recipeId;

            logger.LogInformation("Created recipe {RecipeId} from text generation for user {UserId}", recipeId, userId);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating recipe from text");
            return Results.Problem("Failed to generate recipe from text. Please try again.");
        }
    }
}
