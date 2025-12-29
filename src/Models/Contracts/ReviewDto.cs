using System.Text.Json.Serialization;

namespace BabeAlgorithms.Models.Contracts;

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
