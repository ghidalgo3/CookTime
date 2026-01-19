
using System.Security.Claims;
using Azure.Identity;
using Azure.Storage.Blobs;
using babe_algorithms.Models;
using babe_algorithms.Services;
using babe_algorithms.ViewComponents;
using BabeAlgorithms.Models.Contracts;
using BabeAlgorithms.Routes;
using BabeAlgorithms.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    var keyVaultName = builder.Configuration["KeyVaultName"];
    if (!string.IsNullOrEmpty(keyVaultName))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri($"https://{keyVaultName}.vault.azure.net/"),
            new DefaultAzureCredential());
    }
}

var connectionString = builder.Configuration.GetConnectionString("Postgres")!;
builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddSingleton<CookTimeDB>();
builder.Services.AddSingleton<NutritionService>();
builder.Services.AddSingleton<AIRecipeService>();

// Azure Blob Storage
var blobConnectionString = builder.Configuration.GetConnectionString("AzureBlobStorage");
if (!string.IsNullOrEmpty(blobConnectionString))
{
    var blobServiceClient = new BlobServiceClient(blobConnectionString);
    var blobContainer = blobServiceClient.GetBlobContainerClient("images");
    builder.Services.AddSingleton(blobContainer);
}

builder.Services.AddGoogleAuthentication(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();



// Must be first middleware for OAuth to work correctly behind proxy/Docker
app.UseForwardedHeaders();


app.MapGet("/api/category/list", () => Constants.DefaultCategories);
app.MapGet("/api/recipe/tags", async (CookTimeDB cooktime, string? query) =>
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return await cooktime.GetAllCategoriesWithIdsAsync();
    }
    return await cooktime.SearchCategoriesAsync(query);
});
app.MapGet("/api/recipe/ingredients", async (CookTimeDB cooktime, string? name) =>
{
    if (string.IsNullOrWhiteSpace(name))
    {
        return Results.Ok(Array.Empty<IngredientAutosuggestDto>());
    }
    var results = await cooktime.SearchIngredientsAsync(name);
    return Results.Ok(results);
});
app.MapGet(
    "/api/multipartrecipe",
    async (CookTimeDB cooktime, string? search, int page = 1, int pageSize = 30) =>
{
    List<RecipeSummaryDto> queried_recipes;
    long total_count;

    if (!string.IsNullOrWhiteSpace(search))
    {
        queried_recipes = await cooktime.SearchRecipesAsync(search, pageSize, page);
        total_count = queried_recipes.Count;
    }
    else
    {
        queried_recipes = await cooktime.GetRecipesAsync(pageSize, page);
        total_count = await cooktime.GetRecipeCountAsync();
    }
    return new PagedResult<RecipeSummaryDto>
    {
        Results = queried_recipes,
        CurrentPage = page,
        PageCount = (int)Math.Ceiling((double)total_count / pageSize),
        PageSize = pageSize,
        RowCount = (int)total_count
    };
});
app.MapGet("/api/multipartrecipe/new", async (CookTimeDB cooktime) =>
{
    return await cooktime.GetNewRecipesAsync();
});
app.MapGet("/api/multipartrecipe/featured", async (CookTimeDB cooktime) =>
{
    return await cooktime.GetFeaturedRecipesAsync();
});
app.MapGet("/api/multipartrecipe/{id:guid}", async (CookTimeDB cooktime, NutritionService nutrition, Guid id) =>
{
    var recipe = await cooktime.GetRecipeByIdAsync(id);
    if (recipe == null)
    {
        return Results.NotFound();
    }
    recipe = await nutrition.EnrichWithDensitiesAsync(recipe);
    return Results.Ok(recipe);
});

app.MapGet("/api/multipartrecipe/{id:guid}/reviews", async (CookTimeDB cooktime, Guid id) =>
{
    var reviews = await cooktime.GetReviewsByRecipeIdAsync(id);
    return Results.Ok(reviews);
});

app.MapGet("/api/multipartrecipe/{id:guid}/images", async (CookTimeDB cooktime, Guid id) =>
{
    var images = await cooktime.GetImagesByRecipeIdAsync(id);
    return Results.Ok(images);
});

app.MapGet("/api/multipartrecipe/{id:guid}/nutritionData", async (NutritionService nutrition, Guid id) =>
{
    var nutritionFacts = await nutrition.GetRecipeNutritionFactsAsync(id);
    if (nutritionFacts == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(nutritionFacts);
});

// Admin-only routes (require Administrator role)
var adminApi = app.MapGroup("/api")
    .AddEndpointFilter(async (context, next) =>
    {
        var httpContext = context.HttpContext;
        if (!httpContext.User.IsInRole("Administrator"))
        {
            return Results.Forbid();
        }
        return await next(context);
    });

adminApi.MapGet("/ingredient/internalUpdate", async (CookTimeDB cooktime) =>
{
    var ingredients = await cooktime.GetIngredientsForAdminAsync();
    return Results.Ok(ingredients);
});

adminApi.MapPost("/ingredient/internalupdate", async (CookTimeDB cooktime, IngredientInternalUpdateDto update) =>
{
    var result = await cooktime.UpdateIngredientInternalAsync(update);
    if (result == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(result);
});

var authenticatedApi = app.MapGroup("/api")
    .AddEndpointFilter(async (context, next) =>
    {
        var httpContext = context.HttpContext;
        var userIdClaim = httpContext.User.FindFirst("db_user_id")?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }
        httpContext.Items["UserId"] = userId;
        return await next(context);
    })
    .MapListRoutes()
    .MapRecipeGenerationRoutes();

authenticatedApi.MapGet("/multipartrecipe/mine", async (HttpContext context, CookTimeDB cooktime, int page = 1, int pageSize = 30) =>
{
    var userId = (Guid)context.Items["UserId"]!;
    var recipes = await cooktime.GetRecipesByUserAsync(userId, pageSize, page);
    return Results.Ok(new PagedResult<RecipeSummaryDto>
    {
        Results = recipes,
        CurrentPage = page,
        PageCount = (int)Math.Ceiling((double)recipes.Count / pageSize),
        PageSize = pageSize,
        RowCount = recipes.Count
    });
});

authenticatedApi.MapPost("/multipartrecipe/create", async (HttpContext context, CookTimeDB cooktime, RecipeCreateRequest request) =>
{
    var userId = (Guid)context.Items["UserId"]!;
    var recipeId = await cooktime.CreateRecipeAsync(new RecipeCreateDto
    {
        Name = request.Name,
        OwnerId = userId
    });
    return Results.Ok(new { id = recipeId });
});

authenticatedApi.MapPut("/multipartrecipe/{recipeId:guid}", async (HttpContext context, CookTimeDB cooktime, Guid recipeId, RecipeUpdateDto recipe) =>
{
    var userId = (Guid)context.Items["UserId"]!;

    // Verify the user owns this recipe or is an administrator
    var existingRecipe = await cooktime.GetRecipeByIdAsync(recipeId);
    if (existingRecipe == null)
    {
        return Results.NotFound();
    }
    var isAdmin = context.User.IsInRole("Administrator");
    if (existingRecipe.Owner?.Id != userId && !isAdmin)
    {
        return Results.Forbid();
    }

    recipe.Id = recipeId;
    await cooktime.UpdateRecipeAsync(recipe);
    return Results.Ok();
});

authenticatedApi.MapDelete("/multipartrecipe/{recipeId:guid}", async (HttpContext context, CookTimeDB cooktime, Guid recipeId) =>
{
    var userId = (Guid)context.Items["UserId"]!;

    // Verify the user owns this recipe or is an administrator
    var existingRecipe = await cooktime.GetRecipeByIdAsync(recipeId);
    if (existingRecipe == null)
    {
        return Results.NotFound();
    }
    var isAdmin = context.User.IsInRole("Administrator");
    if (existingRecipe.Owner?.Id != userId && !isAdmin)
    {
        return Results.Forbid();
    }

    await cooktime.DeleteRecipeAsync(recipeId);
    return Results.NoContent();
});

authenticatedApi.MapPut("/multipartrecipe/{recipeId:guid}/image", async (
    HttpContext context,
    CookTimeDB cooktime,
    BlobContainerClient blobContainer,
    ILogger<Program> logger,
    Guid recipeId) =>
{
    var userId = (Guid)context.Items["UserId"]!;

    // Verify the user owns this recipe or is an administrator
    var existingRecipe = await cooktime.GetRecipeByIdAsync(recipeId);
    if (existingRecipe == null)
    {
        return Results.NotFound();
    }
    var isAdmin = context.User.IsInRole("Administrator");
    if (existingRecipe.Owner?.Id != userId && !isAdmin)
    {
        return Results.Forbid();
    }

    // Parse multipart form data
    var form = await context.Request.ReadFormAsync();
    var file = form.Files.GetFile("files");
    if (file == null || file.Length == 0)
    {
        return Results.BadRequest(new { error = "No file provided" });
    }

    // Validate file type
    var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
    if (!allowedTypes.Contains(file.ContentType))
    {
        return Results.BadRequest(new { error = "Invalid file type. Allowed: JPEG, PNG, WebP" });
    }

    // Validate file size (5MB max)
    if (file.Length > 5 * 1024 * 1024)
    {
        return Results.BadRequest(new { error = "File too large. Maximum size is 5MB" });
    }

    try
    {
        // Generate unique blob name
        var imageId = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension))
        {
            extension = file.ContentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
        }
        var blobName = $"{imageId}{extension}";

        // Upload to blob storage
        var blobClient = blobContainer.GetBlobClient(blobName);
        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, overwrite: true);

        // Get the storage URL (replace azurite with localhost for local dev)
        var storageUrl = blobClient.Uri.ToString().Replace("azurite", "localhost");

        // Create image record in database
        await cooktime.CreateImageAsync(imageId, storageUrl, recipeId);

        logger.LogInformation("Uploaded image {ImageId} for recipe {RecipeId}", imageId, recipeId);
        return Results.Ok(new { id = imageId, url = storageUrl });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to upload image for recipe {RecipeId}", recipeId);
        return Results.Problem("Failed to upload image");
    }
}).DisableAntiforgery();

authenticatedApi.MapPut("/multipartrecipe/{recipeId:guid}/review", async (HttpContext context, CookTimeDB cooktime, Guid recipeId, ReviewCreateRequest request) =>
{
    var userId = (Guid)context.Items["UserId"]!;
    var reviewId = await cooktime.CreateReviewAsync(recipeId, userId, request.Rating, request.Text);
    return Results.Ok(new { id = reviewId });
});

app.MapGet("/api/recipe/units", () =>
{
    var allUnits = Enum.GetValues<Unit>();
    var body = allUnits.Select(unit =>
    {
        string siType = "count";
        if ((int)unit < 1000)
        {
            siType = "volume";
        }
        else if ((int)unit >= 2000)
        {
            siType = "weight";
        }
        return new
        {
            Name = unit.ToString().ToLowerInvariant(),
            SIType = siType,
            siValue = unit.GetSIValue()
        };
    });
    return body;
});
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CookTime API");
    });
}

app.UseStaticFiles();
app.MapGoogleAuthEndpoints();
// SPA fallback - serve index.html for any unmatched routes
app.MapFallbackToFile("index.html");

using (var scope = app.Services.CreateScope())
{
    var dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    Migrations.RunMigrations(logger, dataSource);

    // Ensure blob container exists
    var blobContainer = scope.ServiceProvider.GetService<BlobContainerClient>();
    if (blobContainer != null)
    {
        await blobContainer.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
    }

    // await Loader.LoadAsync(dataSource, blobContainer, builder.Environment.ContentRootPath);
}
app.Run();
