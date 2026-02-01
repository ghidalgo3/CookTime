using babe_algorithms.Models;
using BabeAlgorithms.Models.Contracts;
using Npgsql;

namespace babe_algorithms.Services;

public class NutritionService(NpgsqlDataSource dataSource)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Enriches a recipe's ingredient data with computed densities from USDA nutrition data.
    /// </summary>
    public async Task<RecipeDetailDto> EnrichWithDensitiesAsync(RecipeDetailDto recipe)
    {
        var nutritionData = await GetRecipeNutritionDataAsync(recipe.Id);
        if (nutritionData == null)
        {
            return recipe;
        }

        // Build a lookup of ingredient ID -> computed density
        var densityLookup = new Dictionary<Guid, double>();
        foreach (var component in nutritionData.Components)
        {
            foreach (var ingredient in component.Ingredients)
            {
                if (ingredient.IngredientId == null)
                {
                    continue;
                }

                var usdaData = DeserializeNutritionData(ingredient.NutritionFacts);
                if (usdaData != null)
                {
                    densityLookup[ingredient.IngredientId.Value] = usdaData.CalculateDensity();
                }
            }
        }

        // Apply densities to the recipe DTO
        foreach (var component in recipe.RecipeComponents ?? [])
        {
            foreach (var ingredientReq in component.Ingredients ?? [])
            {
                if (ingredientReq.Ingredient != null && densityLookup.TryGetValue(ingredientReq.Ingredient.Id, out var density))
                {
                    ingredientReq.Ingredient.DensityKgPerL = density;
                }
            }
        }

        return recipe;
    }

    public async Task<RecipeNutritionFacts?> GetRecipeNutritionFactsAsync(Guid recipeId)
    {
        var nutritionData = await GetRecipeNutritionDataAsync(recipeId);
        if (nutritionData == null)
        {
            return null;
        }

        var servings = nutritionData.Servings ?? 1;
        var componentNutrition = new List<NutritionFactVector>();
        var ingredientDescriptions = new List<IngredientNutritionDescription>();
        var totalNutrition = new NutritionFactVector();

        foreach (var component in nutritionData.Components)
        {
            var componentTotal = new NutritionFactVector();

            foreach (var ingredient in component.Ingredients)
            {
                var ingredientNutrition = CalculateIngredientNutrition(ingredient);
                componentTotal = componentTotal + ingredientNutrition;

                // Build ingredient description
                var description = BuildIngredientDescription(ingredient, ingredientNutrition.Calories, servings);
                ingredientDescriptions.Add(description);
            }

            componentNutrition.Add(componentTotal);
            totalNutrition = totalNutrition + componentTotal;
        }

        // Divide by servings to get per-serving values
        return new RecipeNutritionFacts
        {
            Recipe = totalNutrition,
            Components = componentNutrition.ToList(),
            Ingredients = ingredientDescriptions,
            DietDetails = [] // TODO: Implement diet analysis
        };
    }

    private async Task<RecipeNutritionDataDto?> GetRecipeNutritionDataAsync(Guid recipeId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand("SELECT cooktime.get_recipe_nutrition_data($1)", conn);
        cmd.Parameters.AddWithValue(recipeId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
        {
            return null;
        }

        return JsonSerializer.Deserialize<RecipeNutritionDataDto>(result.ToString()!, JsonOptions);
    }

    private static USDANutritionData? DeserializeNutritionData(NutritionFactsDto? nutritionFacts)
    {
        if (nutritionFacts?.NutritionData is not { } nutritionData)
        {
            return null;
        }

        var json = nutritionData.GetRawText();

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        USDANutritionData result = nutritionFacts.Dataset switch
        {
            "usda_sr_legacy" => JsonSerializer.Deserialize<StandardReferenceNutritionData>(json, JsonOptions),
            "usda_branded" => JsonSerializer.Deserialize<BrandedNutritionData>(json, JsonOptions),
            _ => throw new NotSupportedException($"Unsupported nutrition dataset: {nutritionFacts.Dataset}")
        };
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        if (result is StandardReferenceNutritionData srData)
        {
            srData.CountRegex = nutritionFacts.CountRegex;
        }
        return result;

    }

    private static NutritionFactVector CalculateIngredientNutrition(IngredientNutritionDto ingredient)
    {
        var nutrition = new NutritionFactVector();

        var usdaData = DeserializeNutritionData(ingredient.NutritionFacts);
        if (usdaData == null)
        {
            return nutrition;
        }

        // Get the multiplier based on unit and quantity
        var multiplier = CalculateNutritionMultiplier(ingredient, usdaData);

        // Extract nutrients from the USDA data
        nutrition.Calories = GetNutrientValue(usdaData.FoodNutrients, "Energy", "kcal") * multiplier;
        nutrition.Proteins = GetNutrientValue(usdaData.FoodNutrients, "Protein", "g") * multiplier;
        nutrition.Carbohydrates = GetNutrientValue(usdaData.FoodNutrients, "Carbohydrate, by difference", "g") * multiplier;
        nutrition.MonoUnsaturatedFats = GetNutrientValue(usdaData.FoodNutrients, "Fatty acids, total monounsaturated", "g") * multiplier;
        nutrition.PolyUnsaturatedFats = GetNutrientValue(usdaData.FoodNutrients, "Fatty acids, total polyunsaturated", "g") * multiplier;
        nutrition.SaturatedFats = GetNutrientValue(usdaData.FoodNutrients, "Fatty acids, total saturated", "g") * multiplier;
        nutrition.TransFats = GetNutrientValue(usdaData.FoodNutrients, "Fatty acids, total trans", "g") * multiplier;
        nutrition.Sugars = GetNutrientValue(usdaData.FoodNutrients, "Sugars, total including NLEA", "g") * multiplier;
        nutrition.Iron = GetNutrientValue(usdaData.FoodNutrients, "Iron, Fe", "mg") * multiplier;
        nutrition.VitaminD = GetNutrientValue(usdaData.FoodNutrients, "Vitamin D (D2 + D3)", "µg") * multiplier;
        nutrition.Calcium = GetNutrientValue(usdaData.FoodNutrients, "Calcium, Ca", "mg") * multiplier;
        nutrition.Potassium = GetNutrientValue(usdaData.FoodNutrients, "Potassium, K", "mg") * multiplier;

        return nutrition;
    }

    private static double CalculateNutritionMultiplier(IngredientNutritionDto ingredient, USDANutritionData usdaData)
    {
        var quantity = ingredient.Quantity ?? 0;
        if (quantity == 0)
        {
            return 0;
        }

        var unitString = ingredient.Unit?.ToLowerInvariant() ?? "count";
        if (!Enum.TryParse<Unit>(unitString, ignoreCase: true, out var unit))
        {
            unit = Unit.Count;
        }

        // USDA data is normalized to 100g
        // We need to convert our quantity to kilograms, then multiply by 10 (since 100g = 0.1kg)
        if (unit.IsMass())
        {
            var kilograms = unit.GetSIValue() * quantity;
            return kilograms * 10; // * 10 because USDA data is per 100g
        }
        else if (unit.IsVolume())
        {
            // Use density from the USDA data to convert volume to mass
            var density = usdaData.CalculateDensity();
            var liters = unit.GetSIValue() * quantity;
            var kilograms = liters * density;
            return kilograms * 10;
        }
        else
        {
            // Count - use unit mass from USDA data, fall back to ingredient's expected mass
            var unitMass = usdaData.CalculateUnitMass() ?? ingredient.ExpectedUnitMassKg ?? 0.1;
            var kilograms = unitMass * quantity;
            return kilograms * 10;
        }
    }

    private static double GetNutrientValue(JsonDocument foodNutrients, string nutrientName, string unitName)
    {
        foreach (var nutrient in foodNutrients.RootElement.EnumerateArray())
        {
            if (!nutrient.TryGetProperty("nutrient", out var nutrientInfo))
            {
                continue;
            }

            var name = nutrientInfo.GetProperty("name").GetString();
            var unit = nutrientInfo.GetProperty("unitName").GetString();

            if (name == nutrientName && unit == unitName)
            {
                if (nutrient.TryGetProperty("amount", out var amount))
                {
                    return amount.GetDouble();
                }
            }
        }

        return 0;
    }

    private static IngredientNutritionDescription BuildIngredientDescription(
        IngredientNutritionDto ingredient,
        double totalCalories,
        int servings)
    {
        var usdaData = DeserializeNutritionData(ingredient.NutritionFacts);

        return new IngredientNutritionDescription
        {
            NutritionDatabaseId = usdaData?.FdcId.ToString() ?? "",
            NutritionDatabaseDescriptor = usdaData?.Description ?? "Unknown",
            Name = ingredient.IngredientName ?? "Unknown",
            Unit = ingredient.Unit ?? "count",
            Quantity = ingredient.Quantity ?? 0,
            Modifier = usdaData?.GetCountModifier() ?? "",
            CaloriesPerServing = totalCalories / servings
        };
    }

}

#region DTOs for deserializing SQL results

internal class RecipeNutritionDataDto
{
    public Guid RecipeId { get; set; }
    public int? Servings { get; set; }
    public List<ComponentNutritionDto> Components { get; set; } = [];
}

internal class ComponentNutritionDto
{
    public Guid ComponentId { get; set; }
    public string? ComponentName { get; set; }
    public int Position { get; set; }
    public List<IngredientNutritionDto> Ingredients { get; set; } = [];
}

internal class IngredientNutritionDto
{
    public Guid IngredientRequirementId { get; set; }
    public Guid? IngredientId { get; set; }
    public string? IngredientName { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public int Position { get; set; }
    public double? ExpectedUnitMassKg { get; set; }
    public NutritionFactsDto? NutritionFacts { get; set; }
}

internal class NutritionFactsDto
{
    public Guid Id { get; set; }
    public JsonElement? SourceIds { get; set; }
    public double? UnitMass { get; set; }
    public double? Density { get; set; }
    public string? Dataset { get; set; }
    public JsonElement? NutritionData { get; set; }
    public string? CountRegex { get; set; }
}

#endregion
