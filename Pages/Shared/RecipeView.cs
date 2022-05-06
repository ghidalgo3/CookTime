using System.Diagnostics.CodeAnalysis;

namespace babe_algorithms.Pages;

public record RecipeView(
    string Name,
    Guid Id,
    List<Guid> ImageIds,
    List<string> Categories,
    double AverageReviews,
    int ReviewCount,
    bool? IsFavorite) {}

public class RecipeViewEqualityComparer : IEqualityComparer<RecipeView>
{
    public bool Equals(RecipeView x, RecipeView y) =>
        x.Id == y.Id;

    public int GetHashCode([DisallowNull] RecipeView obj) => 
        obj.Id.GetHashCode();
}