using Azure.Storage.Blobs;
using babe_algorithms.Models;
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
        var srLegacyPath = Path.Combine(dataDirectory, "FoodData_Central_sr_legacy_food_json_2021-10-28.json");
        var brandedPath = Path.Combine(dataDirectory, "FoodData_Central_branded_food_json_2025-12-18.json");

        // Ensure blob container exists with public access for images
        await blobContainer.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

        await using var conn = await dataSource.OpenConnectionAsync();

        // // 0. Load users first (recipes depend on owner_id)
        // Console.WriteLine("Loading users...");
        // var userCount = await LoadUsersAsync(conn, usersPath);
        // Console.WriteLine($"  Loaded {userCount} users");

        // // 1. Load ingredients
        // Console.WriteLine("Loading ingredients...");
        // var ingredientCount = await LoadIngredientsAsync(conn, ingredientsPath);
        // Console.WriteLine($"  Loaded {ingredientCount} ingredients");

        // // 2. Load recipes (includes categories and components)
        // Console.WriteLine("Loading recipes...");
        // var recipeCount = await LoadRecipesAsync(conn, recipesPath);
        // Console.WriteLine($"  Loaded {recipeCount} recipes");

        // // 3. Load ingredient requirements (links ingredients to recipe components)
        // Console.WriteLine("Loading ingredient requirements...");
        // var ingredientReqCount = await LoadIngredientRequirementsAsync(conn, ingredientRequirementsPath);
        // Console.WriteLine($"  Loaded {ingredientReqCount} ingredient requirements");

        // // 4. Upload images and insert image records
        // Console.WriteLine("Loading images...");
        // var imageCount = await LoadImagesAsync(conn, blobContainer, imagesPath, imagesDirectory);
        // Console.WriteLine($"  Loaded {imageCount} images");

        // // 5. Load reviews
        // Console.WriteLine("Loading reviews...");
        // var reviewCount = await LoadReviewsAsync(conn, reviewsPath);
        // Console.WriteLine($"  Loaded {reviewCount} reviews");

        // 6. Load USDA nutrition data (SR Legacy)
        Console.WriteLine("Loading USDA SR Legacy nutrition data...");
        var srLegacyCount = await LoadNutritionFactsAsync(dataSource, srLegacyPath, "usda_sr_legacy");
        Console.WriteLine($"  Loaded {srLegacyCount} SR Legacy nutrition facts");

        // 7. Load USDA nutrition data (Branded)
        Console.WriteLine("Loading USDA Branded nutrition data...");
        var brandedCount = await LoadNutritionFactsAsync(dataSource, brandedPath, "usda_branded");
        Console.WriteLine($"  Loaded {brandedCount} Branded nutrition facts");

        // 8. Compute and update density values for nutrition facts
        Console.WriteLine("Computing density values for nutrition facts...");
        var densityCount = await ComputeNutritionFactsDensityAsync(dataSource);
        Console.WriteLine($"  Updated {densityCount} nutrition facts with density values");

        // 9. Associate ingredients with nutrition facts
        var ingredientsSimplePath = Path.Combine(dataDirectory, "ingredients_simple.json");
        Console.WriteLine("Associating ingredients with nutrition facts...");
        var associationCount = await AssociateIngredientsWithNutritionFactsAsync(dataSource, ingredientsSimplePath);
        Console.WriteLine($"  Associated {associationCount} ingredients with nutrition facts");

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

    private static async Task<int> LoadNutritionFactsAsync(NpgsqlDataSource dataSource, string filePath, string dataset)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"  Warning: {filePath} not found, skipping {dataset} nutrition facts");
            return 0;
        }

        var totalCount = 0;
        var lineNumber = 0;
        const int batchSize = 500;
        var batch = new List<string>(batchSize);

        await foreach (var line in File.ReadLinesAsync(filePath))
        {
            lineNumber++;

            // Skip first line (opening JSON array) and last line (closing JSON array)
            if (lineNumber == 1)
            {
                continue;
            }

            // Skip empty lines or the closing bracket
            if (string.IsNullOrWhiteSpace(line) || line.Trim() == "]}" || line.Trim() == "]")
            {
                continue;
            }

            // Remove trailing comma if present (NDJSON-style from JSON array)
            var jsonLine = line.TrimEnd().TrimEnd(',');
            batch.Add(jsonLine);

            // Process batch when full
            if (batch.Count >= batchSize)
            {
                var imported = await ImportNutritionBatchAsync(dataSource, batch, dataset);
                totalCount += imported;
                batch.Clear();

                Console.WriteLine($"  Loaded {totalCount} {dataset} records...");
            }
        }

        // Process remaining batch
        if (batch.Count > 0)
        {
            var imported = await ImportNutritionBatchAsync(dataSource, batch, dataset);
            totalCount += imported;
            Console.WriteLine($"  Loaded {totalCount} {dataset} records (final batch)");
        }

        return totalCount;
    }

    private static async Task<int> ImportNutritionBatchAsync(NpgsqlDataSource dataSource, List<string> batch, string dataset)
    {
        try
        {
            await using var conn = await dataSource.OpenConnectionAsync();

            // Build a JSON array from the batch
            var jsonArray = "[" + string.Join(",", batch) + "]";

            await using var cmd = new NpgsqlCommand(
                "SELECT imported_count FROM cooktime.batch_import_nutrition_facts($1::jsonb, $2)",
                conn);
            cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, jsonArray);
            cmd.Parameters.AddWithValue(NpgsqlDbType.Text, dataset);

            var result = await cmd.ExecuteScalarAsync();
            return result is int count ? count : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ERROR importing batch: {ex.Message}");
            // Fall back to individual imports
            return await ImportNutritionIndividuallyAsync(dataSource, batch, dataset);
        }
    }

    private static async Task<int> ImportNutritionIndividuallyAsync(NpgsqlDataSource dataSource, List<string> batch, string dataset)
    {
        var count = 0;
        foreach (var jsonLine in batch)
        {
            try
            {
                await using var conn = await dataSource.OpenConnectionAsync();
                await using var cmd = new NpgsqlCommand("SELECT cooktime.import_nutrition_facts($1::jsonb, $2)", conn);
                cmd.Parameters.AddWithValue(NpgsqlDbType.Jsonb, jsonLine);
                cmd.Parameters.AddWithValue(NpgsqlDbType.Text, dataset);

                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    count++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR on individual record: {ex.Message}");
            }
        }
        return count;
    }

    private static async Task<int> AssociateIngredientsWithNutritionFactsAsync(NpgsqlDataSource dataSource, string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"  Warning: {filePath} not found, skipping ingredient associations");
            return 0;
        }

        var count = 0;
        await foreach (var line in File.ReadLinesAsync(filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var ingredient = JsonSerializer.Deserialize<IngredientSimpleImport>(line, JsonOptions);
                if (ingredient == null)
                {
                    continue;
                }

                await using var conn = await dataSource.OpenConnectionAsync();

                // Try SR Legacy first (by NDB number)
                if (ingredient.NutritionDataNdbNumber.HasValue)
                {
                    await using var srCmd = new NpgsqlCommand(
                        "SELECT cooktime.associate_ingredient_sr_legacy($1, $2)", conn);
                    srCmd.Parameters.AddWithValue(ingredient.Id);
                    srCmd.Parameters.AddWithValue(ingredient.NutritionDataNdbNumber.Value);

                    var srResult = await srCmd.ExecuteScalarAsync();
                    if (srResult is true)
                    {
                        count++;
                        continue; // Successfully associated with SR Legacy
                    }
                }

                // Try Branded (by GTIN/UPC)
                if (!string.IsNullOrEmpty(ingredient.BrandedNutritionDataGtinUpc))
                {
                    await using var brandedCmd = new NpgsqlCommand(
                        "SELECT cooktime.associate_ingredient_branded($1, $2)", conn);
                    brandedCmd.Parameters.AddWithValue(ingredient.Id);
                    brandedCmd.Parameters.AddWithValue(ingredient.BrandedNutritionDataGtinUpc);

                    var brandedResult = await brandedCmd.ExecuteScalarAsync();
                    if (brandedResult is true)
                    {
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR associating ingredient: {ex.Message}");
            }
        }

        return count;
    }

    private static async Task<int> ComputeNutritionFactsDensityAsync(NpgsqlDataSource dataSource)
    {
        var updates = new List<(Guid Id, double Density)>();

        // Fetch all nutrition_facts records that don't have density computed
        await using (var conn = await dataSource.OpenConnectionAsync())
        {
            var selectCmd = new NpgsqlCommand(
                "SELECT id, nutrition_data, dataset FROM cooktime.nutrition_facts WHERE density IS NULL", conn);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var id = reader.GetGuid(0);
                var nutritionDataJson = reader.GetString(1);
                var dataset = reader.GetString(2);

                try
                {
                    double density = 1.0;

                    if (dataset == "usda_sr_legacy")
                    {
                        var srData = JsonSerializer.Deserialize<StandardReferenceNutritionData>(nutritionDataJson, JsonOptions);
                        if (srData != null)
                        {
                            density = srData.CalculateDensity();
                        }
                    }
                    else if (dataset == "usda_branded")
                    {
                        var brandedData = JsonSerializer.Deserialize<BrandedNutritionData>(nutritionDataJson, JsonOptions);
                        if (brandedData != null)
                        {
                            density = brandedData.CalculateDensity();
                        }
                    }

                    updates.Add((id, density));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ERROR computing density for {id}: {ex.Message}");
                }
            }
        }

        // Update the density values in batches
        const int batchSize = 100;
        var updated = 0;
        for (var i = 0; i < updates.Count; i += batchSize)
        {
            var batch = updates.Skip(i).Take(batchSize).ToList();
            await using var conn = await dataSource.OpenConnectionAsync();

            foreach (var (id, density) in batch)
            {
                await using var updateCmd = new NpgsqlCommand(
                    "SELECT cooktime.update_nutrition_facts_density($1, $2)", conn);
                updateCmd.Parameters.AddWithValue(id);
                updateCmd.Parameters.AddWithValue(density);
                await updateCmd.ExecuteNonQueryAsync();
                updated++;
            }

            if (updated % 1000 == 0)
            {
                Console.WriteLine($"  Updated {updated} density values...");
            }
        }

        return updates.Count;
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

file record IngredientSimpleImport
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public int? NutritionDataNdbNumber { get; init; }
    public string? BrandedNutritionDataGtinUpc { get; init; }
    public double? ExpectedUnitMass { get; init; }
}

#endregion