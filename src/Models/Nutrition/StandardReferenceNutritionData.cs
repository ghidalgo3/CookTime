
// using System.Text.Json.Nodes;
// using System.Text.RegularExpressions;

// namespace babe_algorithms.Models;

// public class StandardReferenceNutritionData : USDANutritionData
// {
//     public const string FruitsAndFruitJuices = "Fruits and Fruit Juices";
//     public const string VegetableAndVegetableProducts = "Vegetables and Vegetable Products";
//     public const string LegumeAndLegumeProducts = "Legumes and Legume Products";
//     public const string SpicesAndHerbs = "Spices and Herbs";
//     public const string NutAndSeedProducts = "Nut and Seed Products";
//     public const string CerealGrainsAndPasta = "Cereal Grains and Pasta";
//     public static readonly List<string> PlantBasedCategories = new()
//     {
//         "Beverages",
//         "Baked Products",
//         CerealGrainsAndPasta,
//         FruitsAndFruitJuices,
//         VegetableAndVegetableProducts,
//         "Fats and Oils",
//         "Sweets",
//         "Breakfast Cereals",
//         "Soups, Sauces, and Gravies",
//         "American Indian/Alaska Native Foods",
//         SpicesAndHerbs,
//         NutAndSeedProducts,
//         "Baby Foods",
//         LegumeAndLegumeProducts,
//     };

//     [Key]
//     public int NdbNumber { get; set; }

//     [JsonIgnore]
//     public JsonDocument NutrientConversionFactors { get; set; }

//     [JsonIgnore]
//     public JsonDocument FoodCategory { get; set; }

//     [JsonIgnore]
//     public JsonDocument FoodPortions { get; set; }

//     public string CountRegex { get; set; }

//     public string GetFoodCategoryDescription()
//     {
//         return this.FoodCategory.RootElement.GetProperty("description").GetString() ?? string.Empty;
//     }
//     /// <summary>
//     /// Calculates the density of an ingredient in kilograms per liter.
//     /// </summary>
//     /// <remarks>
//     /// The USDA data is all normalized to 100g of a particular ingredient.
//     /// Not all ingredients are measured by mass in practice,
//     /// some are counts of some kind and some are volumes.
//     /// The density information in encoded in the "food portions" part of the
//     /// SR data. This method attempts to calculate a density based on 
//     /// the different portions in the data set.
//     /// 
//     /// For example NDB 1077 (whole milk) has known portions of 1 cup
//     /// with gram weight of 244g. We know that 1 cup is ~0.236 L
//     /// and we know that 1 cup of milk is 244g, which allows us to compute
//     /// a density of 244g / 0.236L * 1kg/1000g == 1.033 kg/L.
//     /// </remarks>
//     /// <returns></returns>
//     public override double CalculateDensity()
//     {
//         foreach (var portion in this.FoodPortions.RootElement.EnumerateArray())
//         {
//             var modifier = portion.GetProperty("modifier").GetString();
//             if (TryParseUnit(modifier, null, out Unit unit))
//             {
//                 if (unit.IsVolume())
//                 {
//                     return portion.GetProperty("gramWeight").GetDouble() / unit.GetSIValue() / 1000.0;
//                 }
//             }
//         }
//         return 1.0;
//     }

//     public override double? CalculateUnitMass()
//     {
//         foreach (var portion in this.FoodPortions.RootElement.EnumerateArray())
//         {
//             var modifier = portion.GetProperty("modifier").GetString();
//             if (TryParseUnit(modifier, this, out Unit unit))
//             {
//                 if (unit.IsCount())
//                 {
//                     return portion.GetProperty("gramWeight").GetDouble() / 1000.0;
//                 }
//             }
//         }

//         return null;
//     }

//     public override string GetCountModifier()
//     {
//         foreach (var portion in this.FoodPortions.RootElement.EnumerateArray())
//         {
//             var modifier = portion.GetProperty("modifier").GetString();
//             if (TryParseUnit(modifier, this, out Unit unit))
//             {
//                 if (unit.IsCount())
//                 {
//                     return modifier ?? string.Empty;
//                 }
//             }
//         }

//         return string.Empty;
//     }

//     public static bool TryParseUnit(string? modifier, StandardReferenceNutritionData? srData, out Unit unit)
//     {
//         unit = Unit.Count;
//         if (modifier == null) return false;

//         var normalize = modifier.ToUpperInvariant();
//         switch (normalize)
//         {
//             // Volume
//             case "FL OZ":
//                 unit = Unit.FluidOunce;
//                 return true;
//             case "QUART":
//                 unit = Unit.Quart;
//                 return true;
//             case "TBSP":
//                 unit = Unit.Tablespoon;
//                 return true;
//             case "TSP":
//                 unit = Unit.Teaspoon;
//                 return true;
//             case "CUP":
//                 unit = Unit.Cup;
//                 return true;
//             // mass
//             // count
//             default:
//                 break;
//         }

//         if (normalize.Contains("CUP"))
//         {
//             unit = Unit.Cup;
//             return true;
//         }

//         if (!string.IsNullOrWhiteSpace(srData?.CountRegex))
//         {
//             if (Regex.IsMatch(modifier, srData.CountRegex, RegexOptions.IgnoreCase))
//             {
//                 unit = Unit.Count;
//                 return true;
//             }
//         }

//         return false;
//     }

//     public JsonObject ToJsonObject()
//     {
//         var result = new JsonObject
//         {
//             ["ndbNumber"] = this.NdbNumber,
//             ["description"] = this.Description,
//             ["fdcId"] = this.FdcId,
//             ["foodNutrients"] = JsonNode.Parse(this.FoodNutrients.RootElement.GetRawText()),
//             ["nutrientConversionFactors"] = JsonNode.Parse(this.NutrientConversionFactors.RootElement.GetRawText()),
//             ["foodCategory"] = JsonNode.Parse(this.FoodCategory.RootElement.GetRawText()),
//             ["foodPortions"] = JsonNode.Parse(this.FoodPortions.RootElement.GetRawText())
//         };
//         return result;
//     }
// }
