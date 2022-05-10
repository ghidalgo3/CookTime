using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
namespace babe_algorithms.Models;

[Owned]
public class IngredientRequirement : IIngredientRequirement
{
    public Guid Id { get; set; }

    public Ingredient Ingredient { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public Unit Unit { get; set; }

    public double Quantity { get; set; }

    /// <summary>
    /// The position this ingredient should be placed in.
    /// </summary>
    public int Position { get; set; }

    public NutritionFactVector CalculateNutritionFacts()
    {
        throw new NotImplementedException();
    }
}