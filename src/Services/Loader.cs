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
        var ingredientsPath = Path.Combine(dataDirectory, "ingredients.ndjson");
        var recipesPath = Path.Combine(dataDirectory, "recipes.ndjson");
        var imagesPath = Path.Combine(dataDirectory, "images.ndjson");
        var imagesDirectory = Path.Combine(dataDirectory, "images");

        // Ensure blob container exists
        await blobContainer.CreateIfNotExistsAsync();

        await using var conn = await dataSource.OpenConnectionAsync();

        // 1. Load ingredients
        Console.WriteLine("Loading ingredients...");
        var ingredientCount = await LoadIngredientsAsync(conn, ingredientsPath);
        Console.WriteLine($"  Loaded {ingredientCount} ingredients");

        // 2. Load recipes (includes categories)
        Console.WriteLine("Loading recipes...");
        var recipeCount = await LoadRecipesAsync(conn, recipesPath);
        Console.WriteLine($"  Loaded {recipeCount} recipes");

        // 3. Upload images and insert image records
        Console.WriteLine("Loading images...");
        var imageCount = await LoadImagesAsync(conn, blobContainer, imagesPath, imagesDirectory);
        Console.WriteLine($"  Loaded {imageCount} images");

        Console.WriteLine("Data loading complete!");
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

            // Find the image file
            var imageFileName = $"{image.Name}.jpg";
            var imagePath = Path.Combine(imagesDirectory, imageFileName);

            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"  Warning: Image file not found: {imagePath}");
                continue;
            }

            // Upload to blob storage
            var blobClient = blobContainer.GetBlobClient(imageFileName);

            if (!await blobClient.ExistsAsync())
            {
                await using var stream = File.OpenRead(imagePath);
                await blobClient.UploadAsync(stream, overwrite: false);
            }

            var storageUrl = blobClient.Uri.ToString();

            // Insert image record using import function (PascalCase keys to match SQL function)
            var imageJson = JsonSerializer.Serialize(new
            {
                Id = image.Id,
                StorageUrl = storageUrl,
                UploadedDate = image.LastModifiedAt ?? DateTimeOffset.UtcNow
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
}

#endregion