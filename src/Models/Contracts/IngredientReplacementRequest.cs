namespace babe_algorithms.Models;

/// <summary>
/// When this is served to the client, KeptId is empty
/// When the client replies, they set both ids
/// </summary>
public record IngredientReplacementRequest
{
    public Guid ReplacedId { get; init; }
    public string Name { get; init; }
    public int Usage { get; init; }
    public Guid KeptId { get; init; }

    public static IngredientReplacementRequest FromIngredient(Ingredient ingredient, int usage) =>
        new()
        {
            ReplacedId = ingredient.Id,
            Name = ingredient.Name,
            Usage = usage,
            KeptId = Guid.Empty,
        };
}