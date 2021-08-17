using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using babe_algorithms.Models;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms
{
    public class Recipe
    {
        [Required]
        public string? Name { get; set; }
        public IList<IngredientRequirement> Ingredients { get; set; }
        public string Directions { get; set; }
        public double ServingsProduced { get; set; }
        public TimeSpan Cooktime { get; set; }
        public double CaloriesPerServing { get; set; }
        public Guid Id { get; set; }
        public ISet<Category> Categories { get; set; }
    }

    [Owned]
    public class IngredientRequirement
    {
        public Ingredient Ingredient { get; set; }
        public Unit Unit { get; set; }
        public double Quantity { get; set; }
    }
}