namespace babe_algorithms;

#nullable enable

public interface IRecipeArtificialIntelligence
{
    /// <summary>
    /// Extracts recipe content out of free-form text.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="ct"></param>
    /// <returns>The extracted recipe from the text, or null if there was no recipe.</returns>
    Task<MultiPartRecipe?> ConvertToRecipeAsync(string text, CancellationToken ct);
}