namespace babe_algorithms.Models;

#nullable enable

public class RecipeComponent : IRecipeComponent<MultiPartRecipeStep, MultiPartIngredientRequirement>
{
    public string Name { get; set; }
    public Guid Id { get; set; }
    public int Position { get; set; }
    // public Guid MultiPartRecipeId { get; set; }
    public List<MultiPartIngredientRequirement> Ingredients { get; set; } = new List<MultiPartIngredientRequirement>();
    public List<MultiPartRecipeStep> Steps { get; set; } = new List<MultiPartRecipeStep>();

    public bool IsEmpty() {
        return string.IsNullOrWhiteSpace(this.Name) && this.Ingredients.Count == 0 && this.Steps.Count == 0;
    }
}