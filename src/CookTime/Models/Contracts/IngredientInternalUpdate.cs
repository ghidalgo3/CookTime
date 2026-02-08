// #nullable enable

// namespace CookTime.Models;

// public record IngredientInternalUpdate
// {
//     public string? NutritionDescription { get; init; }
//     public Guid? IngredientId { get; init; }
//     public int? NdbNumber { get; init; }
//     public string? IngredientNames { get; init; }
//     public string? GtinUpc { get; init; }
//     public string? CountRegex { get; init; }
//     public double? ExpectedUnitMass { get; init; }

//     public static IngredientInternalUpdate FromIngredient(Ingredient ingredient) =>
//         new()
//         {
//             IngredientId = ingredient.Id,
//             NutritionDescription = ingredient.NutritionData?.Description ?? ingredient.BrandedNutritionData?.Description ?? "",
//             NdbNumber = ingredient.NutritionData?.NdbNumber ?? 0,
//             GtinUpc = ingredient.BrandedNutritionData?.GtinUpc ?? "",
//             IngredientNames = ingredient.Name,
//             CountRegex = ingredient.NutritionData?.CountRegex ?? "",
//             ExpectedUnitMass = ingredient.ExpectedUnitMass,
//         };
// }