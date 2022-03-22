
using System.Text.Json;

namespace babe_algorithms.Models;
public class StandardReferenceNutritionData
{
    [Key]
    public int NdbNumber { get; set; }

    public int FdcId { get; set; }

    public string Description { get; set; }

    public JsonDocument FoodNutrients { get; set; }

    public JsonDocument NutrientConversionFactors { get; set; }

    public JsonDocument FoodCategory { get; set; }

    public JsonDocument FoodPortions { get; set; }
}
