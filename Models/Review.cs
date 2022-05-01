using babe_algorithms.Models.Users;

namespace babe_algorithms.Models;

/// <summary>
/// 
/// </summary>
public class Review : IOwned
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset LastModified { get; set; }

    public ApplicationUser Owner { get; set; }

    public MultiPartRecipe Recipe { get; set; }

    public int Rating { get; set; }

    public string Text { get; set; }
}