using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using babe_algorithms.Models;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms.Models
{
    public class Cart
    {
        public Guid Id { get; set; }
        public List<RecipeRequirement> RecipeRequirement { get; set; }
        public DateTime CreateAt { get; set; }
        public bool Active { get; set; }
    }

    [Owned]
    public class RecipeRequirement
    {
        public Guid Id { get; set; }
        public Recipe Recipe { get; set; }
        public double Quantity { get; set; }
    }
}