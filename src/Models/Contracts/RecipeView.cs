using System.Diagnostics.CodeAnalysis;

namespace babe_algorithms.Pages;

public record ImageReference(Guid Id, string Name){}

/// <summary>
/// The partial recipe view works for the first-level EF projection,
/// it should basically just be a SELECT on MultiPartRecipe properties
/// </summary>
public record PartialRecipeView(
    string Name,
    Guid Id,
    IEnumerable<ImageReference> Images,
    IEnumerable<string> Categories,
    double AverageReviews,
    int ReviewCount,
    DateTimeOffset CreationDate
) { }

/// <summary>
/// The full RecipeView can contain expanded properties that are the results
/// of in-memory joins (like favorite status computation for example)
/// </summary>
public record RecipeView(
    string Name,
    Guid Id,
    IEnumerable<ImageReference> Images,
    IEnumerable<string> Categories,
    double AverageReviews,
    int ReviewCount,
    DateTimeOffset CreationDate,
    bool? IsFavorite) :
        PartialRecipeView(Name, Id, Images, Categories, AverageReviews, ReviewCount, CreationDate)
{
    public static RecipeView From(PartialRecipeView from, Cart cart)
    {
        return new RecipeView(
            from.Name,
            from.Id,
            from.Images,
            from.Categories.ToList(),
            from.AverageReviews,
            from.ReviewCount,
            from.CreationDate,
            cart?.ContainsRecipe(from.Id) ?? false);
    }
}

public class RecipeViewEqualityComparer : IEqualityComparer<RecipeView>
{
    public bool Equals(RecipeView x, RecipeView y) =>
        x.Id == y.Id;

    public int GetHashCode([DisallowNull] RecipeView obj) => 
        obj.Id.GetHashCode();
}