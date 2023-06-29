namespace babe_algorithms.Models;

public record IngredientInternalUpdate
{
    public Guid IngredientId { get; init; }
    public int NdbNumber { get; init; }
    public string IngredientNames { get; init; }
    public string GtinUpc { get; init; }
    public string CountRegex { get; init; }
    public double ExpectedUnitMass { get; init; }

    public static IngredientInternalUpdate FromIngredient(Ingredient ingredient) => 
        new()
        {
            IngredientId = ingredient.Id,
            NdbNumber = ingredient.NutritionData?.NdbNumber ?? 0,
            GtinUpc = ingredient.BrandedNutritionData?.GtinUpc ?? "",
            IngredientNames = ingredient.Name,
            CountRegex = ingredient.NutritionData?.CountRegex ?? "",
            ExpectedUnitMass = ingredient.ExpectedUnitMass,
        };
}