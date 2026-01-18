using Azure.Storage.Blobs;
using Npgsql;
using NpgsqlTypes;

namespace babe_algorithms.Services;

public static class Loader
{
    // Use default options (PascalCase) to match NDJSON file format
    private static readonly JsonSerializerOptions JsonOptions = new();

    public static async Task LoadAsync(
        NpgsqlDataSource dataSource,
        BlobContainerClient blobContainer,
        string dataDirectory)
    {
        var usersPath = Path.Combine(dataDirectory, "users.ndjson");
        var ingredientsPath = Path.Combine(dataDirectory, "ingredients.ndjson");
        var recipesPath = Path.Combine(dataDirectory, "recipes.ndjson");
        var ingredientRequirementsPath = Path.Combine(dataDirectory, "ingredient_requirements.ndjson");
        var imagesPath = Path.Combine(dataDirectory, "images.ndjson");
        var reviewsPath = Path.Combine(dataDirectory, "reviews.ndjson");
        var imagesDirectory = Path.Combine(dataDirectory, "images");

        // Ensure blob container exists with public access for images
        await blobContainer.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

        await using var conn = await dataSource.OpenConnectionAsync();

        // 0. Load users first (recipes depend on owner_id)
        Console.WriteLine("Loading users...");
        var userCount = await LoadUsersAsync(conn, usersPath);
        Console.WriteLine($"  Loaded {userCount} users");

        // 1. Load ingredients
        Console.WriteLine("Loading ingredients...");
        var ingredientCount = await LoadIngredientsAsync(conn, ingredientsPath);
        Console.WriteLine($"  Loaded {ingredientCount} ingredients");

        // 2. Load recipes (includes categories and components)
        Console.WriteLine("Loading recipes...");
        var recipeCount = await LoadRecipesAsync(conn, recipesPath);
        Console.WriteLine($"  Loaded {recipeCount} recipes");

        // 3. Load ingredient requirements (links ingredients to recipe components)
        Console.WriteLine("Loading ingredient requirements...");
        var ingredientReqCount = await LoadIngredientRequirementsAsync(conn, ingredientRequirementsPath);
        Console.WriteLine($"  Loaded {ingredientReqCount} ingredient requirements");

        // 4. Upload images and insert image records
        Console.WriteLine("Loading images...");
        var imageCount = await LoadImagesAsync(conn, blobContainer, imagesPath, imagesDirectory);
        Console.WriteLine($"  Loaded {imageCount} images");

        // 5. Load reviews
        Console.WriteLine("Loading reviews...");
        var reviewCount = await LoadReviewsAsync(conn, reviewsPath);
        Console.WriteLine($"  Loaded {reviewCount} reviews");

        Console.WriteLine("Data loading complete!");
    }

    private static async Task<int> LoadUsersAsync(NpgsqlConnection conn, string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"  Warning: {filePath} not found, skipping users");
            return 0;
        }

        var count = 0;
        await foreach (var line in File.ReadLinesAsync(filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            await using var cmd = new NpgsqlCommand("SELECT cooktime.import_user($1::jsonb)", conn);
            cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, line);

            var result = await cmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                count++;
            }
        }

        return count;
    }

    private static async Task<int> LoadIngredientsAsync(NpgsqlConnection conn, string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"  Warning: {filePath} not found, skipping ingredients");
            return 0;
        }

        var count = 0;
        var lineNumber = 0;
        await foreach (var line in File.ReadLinesAsync(filePath))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                // Log first few characters to see what we're parsing
                Console.WriteLine($"  Line {lineNumber}: {line[..Math.Min(100, line.Length)]}...");

                await using var cmd = new NpgsqlCommand("SELECT cooktime.import_ingredient($1::jsonb)", conn);
                cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, line);

                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    count++;
                }
                else
                {
                    Console.WriteLine($"  Line {lineNumber}: import returned null (possibly duplicate)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR on line {lineNumber}: {ex.Message}");
                Console.WriteLine($"  Line content: {line}");
                throw;
            }
        }

        return count;
    }

    private static async Task<int> LoadIngredientRequirementsAsync(NpgsqlConnection conn, string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"  Warning: {filePath} not found, skipping ingredient requirements");
            return 0;
        }

        var count = 0;
        await foreach (var line in File.ReadLinesAsync(filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            await using var cmd = new NpgsqlCommand("SELECT cooktime.import_ingredient_requirement($1::jsonb)", conn);
            cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, line);

            var result = await cmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                count++;
            }
        }

        return count;
    }

    private static async Task<int> LoadRecipesAsync(NpgsqlConnection conn, string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"  Warning: {filePath} not found, skipping recipes");
            return 0;
        }

        // First pass: extract and create all unique categories
        var createdCategories = new HashSet<Guid>();
        await foreach (var line in File.ReadLinesAsync(filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var recipe = JsonSerializer.Deserialize<RecipeImport>(line, JsonOptions);
            if (recipe == null)
            {
                continue;
            }

            foreach (var category in recipe.Categories ?? [])
            {
                if (createdCategories.Contains(category.Id))
                {
                    continue;
                }

                await using var catCmd = new NpgsqlCommand("SELECT cooktime.import_category($1::jsonb)", conn);
                var categoryJson = JsonSerializer.Serialize(category, JsonOptions);
                catCmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, categoryJson);

                await catCmd.ExecuteScalarAsync();
                createdCategories.Add(category.Id);
            }
        }

        // Second pass: import recipes
        var count = 0;
        await foreach (var line in File.ReadLinesAsync(filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            await using var cmd = new NpgsqlCommand("SELECT cooktime.import_recipe($1::jsonb)", conn);
            cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, line);

            var result = await cmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                count++;
            }
        }

        return count;
    }

    private static async Task<int> LoadReviewsAsync(NpgsqlConnection conn, string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"  Warning: {filePath} not found, skipping reviews");
            return 0;
        }

        var count = 0;
        await foreach (var line in File.ReadLinesAsync(filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            await using var cmd = new NpgsqlCommand("SELECT cooktime.import_review($1::jsonb)", conn);
            cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, line);

            var result = await cmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                count++;
            }
        }

        return count;
    }

    private static async Task<int> LoadImagesAsync(
        NpgsqlConnection conn,
        BlobContainerClient blobContainer,
        string imagesNdjsonPath,
        string imagesDirectory)
    {
        if (!File.Exists(imagesNdjsonPath))
        {
            Console.WriteLine($"  Warning: {imagesNdjsonPath} not found, skipping images");
            return 0;
        }

        var count = 0;
        await foreach (var line in File.ReadLinesAsync(imagesNdjsonPath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var image = JsonSerializer.Deserialize<ImageImport>(line, JsonOptions);
            if (image == null)
            {
                continue;
            }

            // Find the image file (files are named by ID)
            var imageFileName = $"{image.Id}.jpg";
            var imagePath = Path.Combine(imagesDirectory, imageFileName);

            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"  Warning: Image file not found: {imagePath}");
                continue;
            }

            // Upload to blob storage (use ID as blob name)
            var blobClient = blobContainer.GetBlobClient(imageFileName);

            if (!await blobClient.ExistsAsync())
            {
                await using var stream = File.OpenRead(imagePath);
                await blobClient.UploadAsync(stream, overwrite: false);
            }

            // Use localhost instead of 127.0.0.1 for Azurite URLs
            var storageUrl = blobClient.Uri.ToString().Replace("azurite", "localhost");

            // Insert image record using import function (PascalCase keys to match SQL function)
            var imageJson = JsonSerializer.Serialize(new
            {
                Id = image.Id,
                StorageUrl = storageUrl,
                UploadedDate = image.LastModifiedAt ?? DateTimeOffset.UtcNow,
                Name = image.Name,
                RecipeId = image.RecipeId
            });

            await using var cmd = new NpgsqlCommand("SELECT cooktime.import_image($1::jsonb)", conn);
            cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, imageJson);

            var result = await cmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                count++;
            }
        }

        return count;
    }
}

#region Import DTOs

file record RecipeImport
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public List<CategoryImport>? Categories { get; init; }
}

file record CategoryImport
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
}

file record ImageImport
{
    public Guid Id { get; init; }
    public DateTimeOffset? LastModifiedAt { get; init; }
    public string Name { get; init; } = null!;
    public Guid? RecipeId { get; init; }
}

#endregion