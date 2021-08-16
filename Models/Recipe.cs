using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms
{
    public class Recipe
    {
        [Required]
        public string? Name { get; set; }
        public Guid Id { get; set; }
    }
}