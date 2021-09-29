using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace babe_algorithms.Models
{
    public class Ingredient
    {
        [Required]
        public string Name { get; set; }
        // public MeasureType MeasureType { get; set; }
        public Guid Id { get; set; }
    }

    // [JsonConverter(typeof())]
    public enum Unit
    {
        // Volumetric Units
        Tablespoon = 100,
        Teaspoon = 101,
        Milliliter = 102,
        Cup = 103,

        // Count
        Count = 1000,

        // Mass
        Ounce = 2000,
    }

    public class Category
    {
        [Required]
        public string Name { get; set; }
        public Guid Id { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public ICollection<Recipe> Recipes { get; set; }
    }
}