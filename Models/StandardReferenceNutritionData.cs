
using System.Text.Json;
using Newtonsoft.Json;
using babe_algorithms.Services;
using Newtonsoft.Json.Linq;

namespace babe_algorithms.Models;
public class StandardReferenceNutritionData
{
    [Key]
    public int NdbNumber { get; set; }

    public int FdcId { get; set; }

    public string Description { get; set; }

    [JsonIgnore]
    public JsonDocument FoodNutrients { get; set; }

    [JsonIgnore]
    public JsonDocument NutrientConversionFactors { get; set; }

    [JsonIgnore]
    public JsonDocument FoodCategory { get; set; }

    [JsonIgnore]
    public JsonDocument FoodPortions { get; set; }

    public JObject ToJObject()
    {
        var result = new JObject
        {
            ["ndbNumber"] = this.NdbNumber,
            ["description"] = this.Description,
            ["fdcId"] = this.FdcId,
            ["foodNutrients"] = JToken.Parse(this.FoodNutrients.RootElement.GetRawText()),
            ["nutrientConversionFactors"] = JToken.Parse(this.NutrientConversionFactors.RootElement.GetRawText()),
            ["foodCategory"] = JToken.Parse(this.FoodCategory.RootElement.GetRawText()),
            ["foodPortions"] = JToken.Parse(this.FoodPortions.RootElement.GetRawText())
        };
        return result;
    }
}
