namespace CookTime.Models.Contracts;

public class IngredientDto
{
    [JsonPropertyName("ingredientId")]
    public int IngredientId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("pluralName")]
    public string? PluralName { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("nutritionFacts")]
    public NutritionDataDto? NutritionFacts { get; set; }
}

public class IngredientCreateDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("pluralName")]
    public string? PluralName { get; set; }
}

// Matches the Autosuggestable type on the frontend
public class IngredientAutosuggestDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("isNew")]
    public bool IsNew { get; set; } = false;
}

// DTO for admin ingredient internal update view
public class IngredientInternalUpdateDto
{
    [JsonPropertyName("ingredientId")]
    public Guid IngredientId { get; set; }

    [JsonPropertyName("ingredientNames")]
    public string IngredientNames { get; set; } = null!;

    [JsonPropertyName("ndbNumber")]
    public int NdbNumber { get; set; }

    [JsonPropertyName("gtinUpc")]
    public string? GtinUpc { get; set; }

    [JsonPropertyName("countRegex")]
    public string? CountRegex { get; set; }

    [JsonPropertyName("expectedUnitMass")]
    public string ExpectedUnitMass { get; set; } = "0.1";

    [JsonPropertyName("nutritionDescription")]
    public string? NutritionDescription { get; set; }
}

// Unified DTO for the combined ingredient admin view
public class IngredientUnifiedDto
{
    [JsonPropertyName("ingredientId")]
    public Guid IngredientId { get; set; }

    [JsonPropertyName("ingredientNames")]
    public string IngredientNames { get; set; } = null!;

    [JsonPropertyName("ndbNumber")]
    public int NdbNumber { get; set; }

    [JsonPropertyName("gtinUpc")]
    public string? GtinUpc { get; set; }

    [JsonPropertyName("countRegex")]
    public string? CountRegex { get; set; }

    [JsonPropertyName("expectedUnitMass")]
    public string ExpectedUnitMass { get; set; } = "0.1";

    [JsonPropertyName("nutritionDescription")]
    public string? NutritionDescription { get; set; }

    [JsonPropertyName("usage")]
    public long Usage { get; set; }

    [JsonPropertyName("hasNutrition")]
    public bool HasNutrition { get; set; }
}

// DTO for ingredient normalizer/replacement
public class IngredientReplacementRequestDto
{
    [JsonPropertyName("replacedId")]
    public Guid ReplacedId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("usage")]
    public long Usage { get; set; }

    [JsonPropertyName("hasNutrition")]
    public bool HasNutrition { get; set; }

    [JsonPropertyName("keptId")]
    public Guid KeptId { get; set; }
}

// DTO for merging ingredients
public class MergeIngredientsDto
{
    [JsonPropertyName("fromIngredientId")]
    public Guid FromIngredientId { get; set; }

    [JsonPropertyName("toIngredientId")]
    public Guid ToIngredientId { get; set; }
}

// DTO for nutrition facts search results
public class NutritionFactsSearchDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("ndbNumber")]
    public int? NdbNumber { get; set; }

    [JsonPropertyName("gtinUpc")]
    public string? GtinUpc { get; set; }

    [JsonPropertyName("dataset")]
    public string Dataset { get; set; } = null!;
}