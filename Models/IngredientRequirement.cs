using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;

namespace babe_algorithms;
[Owned]
public class IngredientRequirement
{
    public Guid Id { get; set; }
    public Ingredient Ingredient { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public Unit Unit { get; set; }
    public double Quantity { get; set; }
}