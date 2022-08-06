using System.ComponentModel.DataAnnotations.Schema;
using babe_algorithms.Models.Users;

namespace babe_algorithms.Models;

/// <summary>
/// Heavily overloaded type, this actually represents a list of recipes.
/// Lists are used to represent 3 important scenarios:
/// 1. Shopping carts
/// 2. Favorites
/// 3. Arbitrary lists
/// </summary>
public class Cart : IOwned
{
    public const string DefaultName = "Cart";
    public const string Favorites = "Favorites";

    public Guid Id { get; set; }

    public string Name { get; set; } = DefaultName;

    public string Description { get; set; }

    public List<RecipeRequirement> RecipeRequirement { get; set; }

    public List<CartIngredient> IngredientState { get; set; }

    public DateTime CreateAt { get; set; }

    public bool Active { get; set; }

    [JsonIgnore]
    public ApplicationUser Owner { get ; set ; }

    public bool ContainsRecipe(MultiPartRecipe recipe)
    {
        return this.RecipeRequirement.Any(rr => rr.MultiPartRecipe.Equals(recipe));
    }

    public bool ContainsRecipe(Guid recipeId)
    {
        return this.RecipeRequirement.Any(rr => rr.MultiPartRecipe.Id.Equals(recipeId));
    }

    [NotMapped]
    public List<DietDetail> DietDetails { get; set; } = new List<DietDetail>();

    public ISet<Ingredient> GetAllIngredients() 
    {
        return this.RecipeRequirement.SelectMany(rr => rr.MultiPartRecipe.GetAllIngredients()).ToHashSet();
    }

}


