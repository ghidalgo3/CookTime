using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using babe_algorithms.Models;

namespace babe_algorithms
{
    public class Recipe
    {
        [Required]
        public string? Name { get; set; }
        public List<IngredientRequirement>? Ingredients { get; set; }
        public List<RecipeStep>? Steps { get; set; }
        public double ServingsProduced { get; set; }
        public TimeSpan Cooktime { get; set; }
        public double CaloriesPerServing { get; set; }
        public Guid Id { get; set; }
        public ISet<Category> Categories { get; set; }
    }
}