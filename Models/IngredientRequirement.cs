using babe_algorithms.Models;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms
{
    [Owned]
    public class IngredientRequirement
    {
        public Ingredient Ingredient { get; set; }
        public Unit Unit { get; set; }
        public double Quantity { get; set; }
    }
}