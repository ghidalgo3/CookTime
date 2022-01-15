namespace babe_algorithms.Models;

public interface IRecipeComponent<TRecipeStep, TIngredientRequirement>
    where TRecipeStep : IRecipeStep
    where TIngredientRequirement : IIngredientRequirement

{
    List<TIngredientRequirement> Ingredients { get; set; }
    List<TRecipeStep> Steps { get; set; }
}

public interface IRecipeStep 
{
    string Text { get; set; }
}

public interface IIngredientRequirement
{
    Guid Id { get; set; }
    Ingredient Ingredient { get; set; }
    double Quantity { get; set; }
    Unit Unit { get; set; }
    int Position { get; set; }
}