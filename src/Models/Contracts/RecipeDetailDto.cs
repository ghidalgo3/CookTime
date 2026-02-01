namespace BabeAlgorithms.Models.Contracts;

public class RecipeDetailDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("owner")]
    public OwnerDto? Owner { get; set; }

    [JsonPropertyName("cooktimeMinutes")]
    public double? CooktimeMinutes { get; set; }

    [JsonPropertyName("caloriesPerServing")]
    public int? CaloriesPerServing { get; set; }

    [JsonPropertyName("servingsProduced")]
    public int? ServingsProduced { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("staticImage")]
    public string? StaticImage { get; set; }

    [JsonPropertyName("recipeComponents")]
    public List<ComponentDetailDto> RecipeComponents { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<CategoryDto> Categories { get; set; } = new();

    [JsonPropertyName("reviewCount")]
    public int ReviewCount { get; set; }

    [JsonPropertyName("averageReviews")]
    public double AverageReviews { get; set; }
}

public class OwnerDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = null!;
}

public class RecipeSummaryDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("images")]
    public List<ImageDto> Images { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; } = new();

    [JsonPropertyName("averageReviews")]
    public double AverageReviews { get; set; }

    [JsonPropertyName("reviewCount")]
    public int ReviewCount { get; set; }

    [JsonPropertyName("cooktimeMinutes")]
    public int? CooktimeMinutes { get; set; }
}

public class ImageDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;
}
public class ImageInfoDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;

    [JsonPropertyName("recipeId")]
    public Guid RecipeId { get; set; }
}