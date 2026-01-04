namespace BabeAlgorithms.Models.Contracts;

public class CategoryDto
{
    [JsonPropertyName("categoryId")]
    public int CategoryId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }
}

public class CategoryCreateDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }
}
