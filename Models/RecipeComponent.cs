namespace babe_algorithms.Models;

public class RecipeComponent
{
    public string Name { get; set; }
    public Guid Id { get; set; }
    public List<IngredientRequirement> Ingredients { get; set; }
    public List<RecipeStep> Steps { get; set; }

}
