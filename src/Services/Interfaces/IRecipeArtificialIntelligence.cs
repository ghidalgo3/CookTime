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

    /// <summary>
    /// Generates one or more images for the recipe.
    /// </summary>
    /// <param name="recipe">Recipe object, needs not exist in the database</param>
    /// <param name="ct">Optional cancellation token</param>
    /// <returns>An awaitable task that yields a list of images.</returns>
    Task<IEnumerable<Models.Image>> GenerateRecipeImageAsync(MultiPartRecipe recipe, CancellationToken ct);
}