using Microsoft.EntityFrameworkCore;

namespace babe_algorithms.Models;

[Owned]
public class RecipeRequirement : IEquatable<RecipeRequirement>
{
    public Guid Id { get; set; }

    public Recipe Recipe { get; set; }

    public MultiPartRecipe MultiPartRecipe { get; set; }

    public double Quantity { get; set; }

    public bool Equals(RecipeRequirement other) => this.Id == other.Id;

    public override int GetHashCode() => this.Id.GetHashCode();
}