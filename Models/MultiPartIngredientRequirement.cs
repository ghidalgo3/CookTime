#nullable enable

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace babe_algorithms.Models;

[Owned]
public class MultiPartIngredientRequirement : IIngredientRequirement
{
    public MultiPartIngredientRequirement()
    {
    }

    public MultiPartIngredientRequirement(IngredientRequirement ir)
    {
        this.Ingredient = ir.Ingredient;
        this.Unit = ir.Unit;
        this.Quantity = ir.Quantity;
        this.Position = ir.Position;
    }

    public Guid Id { get; set; }

    public Ingredient Ingredient { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public Unit Unit { get; set; }

    public double Quantity { get; set; }

    /// <summary>
    /// The position this ingredient should be placed in.
    /// </summary>
    public int Position { get; set; }

    public IngredientNutritionDescription GetPartialIngredientDescription()
    {
        var description = new IngredientNutritionDescription
        {
            Name = this.Ingredient.Name,
            Unit = this.Unit.ToString(),
            Quantity = this.Quantity
        };
        if (this.Ingredient.NormalNutritionData is StandardReferenceNutritionData data)
        {
            description.nutritionDatabaseId = data.FdcId.ToString();
            description.NutritionDatabaseDescriptor = data.Description;
            if (this.Unit.IsCount())
            {
                description.Modifier = data.GetCountModifier();
            }
        }
        else if (this.Ingredient.NormalNutritionData is BrandedNutritionData brandedData)
        {
            description.nutritionDatabaseId = brandedData.FdcId.ToString();
            description.NutritionDatabaseDescriptor = brandedData.Description;
        }
        else
        {
            description.NutritionDatabaseDescriptor = "Unknown";
        }
        return description;
    }

    public NutritionFactVector CalculateNutritionFacts()
    {
        var nutritionFacts = new NutritionFactVector();
        if (this.Ingredient.NormalNutritionData == null)
        {
            return nutritionFacts;
        }
        // find the food nutrient 
        var nutritionData = JToken.Parse(this.Ingredient.NormalNutritionData.FoodNutrients.RootElement.GetRawText());
        this.ComputeNutritionValue(nutritionData, "Energy", "kcal", n => nutritionFacts.Calories = n);
        this.ComputeNutritionValue(nutritionData, "Protein", "g", n => nutritionFacts.Proteins = n);
        this.ComputeNutritionValue(nutritionData, "Carbohydrate, by difference", "g", n => nutritionFacts.Carbohydrates = n);
        this.ComputeNutritionValue(nutritionData, "Fatty acids, total monounsaturated", "g", n => nutritionFacts.MonoUnsaturatedFats = n);
        this.ComputeNutritionValue(nutritionData, "Fatty acids, total polyunsaturated", "g", n => nutritionFacts.PolyUnsaturatedFats = n);
        this.ComputeNutritionValue(nutritionData, "Fatty acids, total saturated",       "g", n => nutritionFacts.SaturatedFats = n);
        this.ComputeNutritionValue(nutritionData, "Fatty acids, total trans",           "g", n => nutritionFacts.TransFats = n);
        this.ComputeNutritionValue(nutritionData, "Sugars, total including NLEA",       "g", n => nutritionFacts.Sugars = n);
        return nutritionFacts;
    }

    private void ComputeNutritionValue(JToken nutritionData, string nutrientName, string unitName, Action<double> propertySetter)
    {
        // this represents nutrients in 100 grams of this ingredient
        var calorieData =  nutritionData.SelectTokens(@$"$[?(@.nutrient.name == '{nutrientName}' && @.nutrient.unitName == '{unitName}')]").FirstOrDefault();
        if (calorieData == null)
        {
            propertySetter.Invoke(0);
            return;
        }

        if (this.Unit.IsMass())
        {
            var kilgramsOfUnit = this.Unit.GetSIValue() * this.Quantity;
            // * 10 because the SR data is for 100g 
            propertySetter.Invoke(kilgramsOfUnit * 10 * calorieData.Value<double>("amount"));
        }
        else if (this.Unit.IsVolume())
        {
            var ingredientDensity = this.Ingredient.NormalNutritionData.CalculateDensity();
            var kilogramsOfUnit = this.Unit.GetSIValue() * this.Quantity * ingredientDensity;
            propertySetter.Invoke(kilogramsOfUnit * 10 * calorieData.Value<double>("amount"));
        }
        else
        {
            var mass = this.Ingredient.NormalNutritionData.CalculateUnitMass() ?? this.Ingredient.ExpectedUnitMass;
            var kilogramsOfUnit = mass * this.Quantity;
            propertySetter.Invoke(kilogramsOfUnit * 10 * calorieData.Value<double>("amount"));
        }
    }
}