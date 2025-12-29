using System.Text.Json;
using babe_algorithms.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace babe_algorithms;

public static class DatabaseExporter
{
    public static void ExportDatabase(string[] args)
    {
        // Enable legacy timestamp behavior
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var builder = WebApplication.CreateBuilder(args);

        // Configure DbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("Postgres");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = System.Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_Postgres")
                    ?? throw new NullReferenceException("Connection string was not found.");
            }
            NpgsqlDataSource dataSource = Program.CreateNpgsqlDataSource(connectionString);
            options.UseNpgsql(dataSource, o => o.MapEnum<Unit>());
        });

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        // Ensure enum types are loaded
        using var conn = (NpgsqlConnection)context.Database.GetDbConnection();
        conn.Open();
        conn.ReloadTypes();

        logger.LogInformation("Starting database export...");

        // Export recipes
        logger.LogInformation("Exporting recipes...");
        var recipes = context.MultiPartRecipes
            .Include(r => r.RecipeComponents)
                .ThenInclude(c => c.Ingredients)
            .Include(r => r.RecipeComponents)
                .ThenInclude(c => c.Steps)
            .Include(r => r.Categories)
            .ToList();

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    static typeInfo =>
                    {
                        if (typeInfo.Kind != System.Text.Json.Serialization.Metadata.JsonTypeInfoKind.Object)
                            return;

                        var propertiesToRemove = typeInfo.Properties
                            .Where(p => p.Name == "ApplicableDefaultCategories"
                                     || p.Name == "Recipes"
                                     || p.Name == "MultiPartRecipes"
                                     || p.Name == "Recipe"
                                     || p.Name == "RecipeComponent"
                                     || p.Name == "Ingredient"
                                     || p.Name == "NutritionData"
                                     || p.Name == "BrandedNutritionData"
                                     || p.Name == "StaticImage"
                                     || p.Name == "SearchVector")
                            .ToList();

                        foreach (var property in propertiesToRemove)
                        {
                            typeInfo.Properties.Remove(property);
                        }
                    }
                }
            }
        };

        using (var recipesFile = File.CreateText("recipes.ndjson"))
        {
            foreach (var recipe in recipes)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(recipe, jsonOptions);
                recipesFile.WriteLine(json);
            }
        }
        logger.LogInformation($"Exported {recipes.Count} recipes to recipes.ndjson");

        // Export ingredients
        logger.LogInformation("Exporting ingredients...");
        var ingredients = context.Ingredients
            .Include(i => i.NutritionData)
            .Include(i => i.BrandedNutritionData)
            .ToList();

        using (var ingredientsFile = File.CreateText("ingredients.ndjson"))
        {
            foreach (var ingredient in ingredients)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(ingredient, jsonOptions);
                ingredientsFile.WriteLine(json);
            }
        }
        logger.LogInformation($"Exported {ingredients.Count} ingredients to ingredients.ndjson");

        // Export images
        logger.LogInformation("Exporting images...");
        var images = context.Images.ToList();

        Directory.CreateDirectory("images");

        using (var imagesFile = File.CreateText("images.ndjson"))
        {
            foreach (var image in images)
            {
                // Save image byte data to file
                if (image.Data != null && image.Data.Length > 0)
                {
                    var extension = DetectImageExtension(image.Data);
                    var filename = $"{image.Id}{extension}";
                    File.WriteAllBytes(Path.Combine("images", filename), image.Data);
                    logger.LogInformation($"Saved image {filename}");
                }

                // Serialize metadata without byte data
                var imageMetadata = new
                {
                    image.Id,
                    image.LastModifiedAt,
                    image.Name,
                };

                var json = System.Text.Json.JsonSerializer.Serialize(imageMetadata, jsonOptions);
                imagesFile.WriteLine(json);
            }
        }
        logger.LogInformation($"Exported {images.Count} images to images.ndjson and images/ directory");

        logger.LogInformation("Database export completed successfully!");
    }

    private static string DetectImageExtension(byte[] imageData)
    {
        if (imageData.Length < 4)
            return ".bin";

        // PNG signature
        if (imageData[0] == 0x89 && imageData[1] == 0x50 && imageData[2] == 0x4E && imageData[3] == 0x47)
            return ".png";

        // JPEG signature
        if (imageData[0] == 0xFF && imageData[1] == 0xD8 && imageData[2] == 0xFF)
            return ".jpg";

        // GIF signature
        if (imageData[0] == 0x47 && imageData[1] == 0x49 && imageData[2] == 0x46)
            return ".gif";

        // WebP signature
        if (imageData.Length >= 12 && imageData[0] == 0x52 && imageData[1] == 0x49 &&
            imageData[2] == 0x46 && imageData[3] == 0x46 &&
            imageData[8] == 0x57 && imageData[9] == 0x45 && imageData[10] == 0x42 && imageData[11] == 0x50)
            return ".webp";

        return ".bin";
    }
}
