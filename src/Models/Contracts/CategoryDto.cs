namespace BabeAlgorithms.Models.Contracts;

public class CategoryDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

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

// Matches the Autosuggestable type on the frontend
public class CategoryWithIdDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("isNew")]
    public bool IsNew { get; set; } = false;
}
