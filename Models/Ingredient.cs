using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace babe_algorithms.Models;
public class Ingredient
{
    [Required]
    public string Name { get; set; }
    // public MeasureType MeasureType { get; set; }
    public Guid Id { get; set; }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum Unit
{
    // Volumetric Units
    Tablespoon = 100,
    Teaspoon = 101,
    Milliliter = 102,
    Cup = 103,
    FluidOunce = 104,
    Pint = 105,
    Quart = 106,
    Gallon = 107,
    Liter = 108,

    // Count
    Count = 1000,

    // Mass
    Ounce = 2000,
    Pound = 2001,
    Milligram = 2002,
    Gram = 2003,
    Kilogram = 2004
}

public class Category
{
    [Required]
    public string Name { get; set; }
    public Guid Id { get; set; }
    [Newtonsoft.Json.JsonIgnore]
    public ICollection<Recipe> Recipes { get; set; }
}