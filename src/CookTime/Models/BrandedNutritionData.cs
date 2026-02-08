namespace CookTime.Models;


public class BrandedNutritionData : USDANutritionData
{
    public required string GtinUpc { get; set; }

    public required string Ingredients { get; set; }

    public double ServingSize { get; set; }

    public required string ServingSizeUnit { get; set; }

    public JsonDocument? LabelNutrients { get; set; }

    public required string BrandedFoodCategory { get; set; }

    public override double CalculateDensity() => 1;

    public override double? CalculateUnitMass()
    {
        if (this.ServingSizeUnit.Equals("g"))
        {
            return this.ServingSize / 1000;
        }
        else
        {
            return 0.1;
        }
    }

    public override string GetCountModifier() => "";
}