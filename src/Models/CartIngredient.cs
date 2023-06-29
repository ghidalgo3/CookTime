using Microsoft.EntityFrameworkCore;

namespace babe_algorithms.Models;

/// <summary>
/// Overrides to cart ingredients
/// </summary>
[Owned]
public class CartIngredient : IEquatable<CartIngredient>
{
    public Guid Id { get; set; }

    public Ingredient Ingredient { get; set; }

    public bool Checked { get; set; }

    public bool Equals(CartIngredient other) => other != null && this.Id == other.Id;

    public override int GetHashCode() => this.Id.GetHashCode();

    public override bool Equals(object obj) => Equals(obj as CartIngredient);
}