using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms.Models;
public class Cart
{
    public Guid Id { get; set; }
    public List<RecipeRequirement> RecipeRequirement { get; set; }
    public List<CartIngredient> IngredientState { get; set; }
    public DateTime CreateAt { get; set; }
    public bool Active { get; set; }
}

[Owned]
public class RecipeRequirement : IEquatable<RecipeRequirement>
{
    public Guid Id { get; set; }
    public Recipe Recipe { get; set; }
    public double Quantity { get; set; }

    public bool Equals(RecipeRequirement other) => this.Id == other.Id;

    public override int GetHashCode() => this.Id.GetHashCode();
}

/// <summary>
/// Overrides to cart ingredients
/// </summary>
[Owned]
public class CartIngredient : IEquatable<CartIngredient>
{
    public Guid Id { get; set; }
    public Ingredient Ingredient { get; set; }
    public bool Checked { get; set; }

    public bool Equals(CartIngredient other) => this.Id == other.Id;

    public override int GetHashCode() => this.Id.GetHashCode();
}