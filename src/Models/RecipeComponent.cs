using System.Diagnostics;

namespace babe_algorithms.Models;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class RecipeComponent : IRecipeComponent<MultiPartRecipeStep, MultiPartIngredientRequirement>
{
    public string Name { get; set; }
    public Guid Id { get; set; }
    public int Position { get; set; }
    public List<MultiPartIngredientRequirement> Ingredients { get; set; } = new List<MultiPartIngredientRequirement>();
    public List<MultiPartRecipeStep> Steps { get; set; } = new List<MultiPartRecipeStep>();

    public bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(this.Name) && this.Ingredients.Count == 0 && this.Steps.Count == 0;
    }

    public override string ToString() => this.Name;
    private string GetDebuggerDisplay() => this.ToString();
}