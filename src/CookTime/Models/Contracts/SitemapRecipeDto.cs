namespace BabeAlgorithms.Models.Contracts;

public class SitemapRecipeDto
{
    public Guid Id { get; set; }
    public DateTime LastModified { get; set; }
}

public class SitemapListDto
{
    public string Slug { get; set; } = null!;
    public DateTime CreationDate { get; set; }
}
