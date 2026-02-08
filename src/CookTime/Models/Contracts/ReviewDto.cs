using System.Text.Json.Serialization;

namespace CookTime.Models.Contracts;

public record ReviewViewDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("owner")]
    public OwnerDto Owner { get; init; } = null!;

    [JsonPropertyName("rating")]
    public int Rating { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }
}

public class ReviewDto
{
    [JsonPropertyName("reviewId")]
    public int ReviewId { get; set; }

    [JsonPropertyName("recipeId")]
    public int RecipeId { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = null!;

    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTimeOffset CreatedDate { get; set; }
}

public class ReviewCreateDto
{
    [JsonPropertyName("recipeId")]
    public int RecipeId { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = null!;

    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}

public record ReviewCreateRequest(
    [property: JsonPropertyName("rating")] int Rating,
    [property: JsonPropertyName("text")] string? Text
);
