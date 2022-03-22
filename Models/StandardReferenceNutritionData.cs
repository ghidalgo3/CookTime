
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
        var result = new JObject();
        result["ndbNumber"] = this.NdbNumber;
        result["description"] = this.Description;
        result["fdcId"] = this.FdcId;
        result["foodNutrients"] = JToken.Parse(this.FoodNutrients.RootElement.GetRawText());
        result["nutrientConversionFactors"] = JToken.Parse(this.NutrientConversionFactors.RootElement.GetRawText());
        result["foodCategory"] = JToken.Parse(this.FoodCategory.RootElement.GetRawText());
        result["foodPortions"] = JToken.Parse(this.FoodPortions.RootElement.GetRawText());
        return result;
    }
}
