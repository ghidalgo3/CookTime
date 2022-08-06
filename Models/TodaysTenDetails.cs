
using System.Text.RegularExpressions;

namespace babe_algorithms.Models;

public class TodaysTenDetails 
{
    public bool HasFruits { get; set; }
    public bool HasVegetables { get; set; }
    public bool HasBeans { get; set; }
    public bool HasHerbsAndSpices { get; set; }
    public bool HasNutsAndSeeds { get; set; }
    public bool HasGrains { get; set; }
    public bool HasFlaxseeds { get; set; }
    public bool HasBerries { get; set; }
    public bool HasGreens { get; set; }
    public bool HasCruciferousVegetables { get; set; }

    private static Regex Greens = new(
        "arugula|greens|beet greens|mustard greens|kale|spring mix|salad|mesclun|collard|sorrel|spinach|swiss chard|turnip greens",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    private static Regex Crucifers = new(
        "arugula|bok choy|broccoli|brussels sprouts|cabbage|cauliflower|collard greens|horseradish|kale|kohlrabi|mustard greens|radish|turnip greens|watercress",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    private static Regex Berries = new(
        "strawberr|blueberr|açai|raspberr|blackberr|cherry",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    public static DietDetail GetTodaysTenDietDetail(ISet<Ingredient> allIngredients)
    {
        Func<string, string, bool> strcmp = (a, b) => string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        Func<Ingredient, string, bool> IsTodaysTen = (ingredient, name) =>
            allIngredients.Any(ingredient => strcmp(ingredient.NutritionData?.GetFoodCategoryDescription(), name));
        // Fruit
        var hasFruit = allIngredients.Any(TodaysTenDetails.IsFruit);
        // Vegetable
        var hasVegetable = allIngredients.Any(ingredient => IsTodaysTen(ingredient, StandardReferenceNutritionData.VegetableAndVegetableProducts));
        // Bean
        var hasBeans = allIngredients.Any(ingredient => IsTodaysTen(ingredient, StandardReferenceNutritionData.LegumeAndLegumeProducts));
        // Spices
        var hasSpices = allIngredients.Any(TodaysTenDetails.IsSpicesAndHerbs);
        // Grain
        var hasGrain = allIngredients.Any(ingredient => IsTodaysTen(ingredient, StandardReferenceNutritionData.CerealGrainsAndPasta));
        // Nuts
        var hasNuts = allIngredients.Any(TodaysTenDetails.IsNutsAndSeeds);
        // Flaxseed
        var hasFlaxseed = allIngredients.Any(TodaysTenDetails.IsFlaxseed);
        // Berry
        var hasBerry = allIngredients.Any(TodaysTenDetails.IsBerry);
        // Greens
        var hasGreens = allIngredients.Any(TodaysTenDetails.IsGreen);
        // Crucifers
        var hasCruciferousVegetables = allIngredients.Any(TodaysTenDetails.IsCruciferousVegetable);
        
        return (new DietDetail()
        {
            Name = "TodaysTen",
            Opinion = DietOpinion.Neutral, // TODO
            Details = new TodaysTenDetails()
            {
                HasFruits = hasFruit,
                HasVegetables = hasVegetable,
                HasBeans = hasBeans,
                HasHerbsAndSpices = hasSpices,
                HasNutsAndSeeds = hasNuts,
                HasGrains = hasGrain,
                HasFlaxseeds = hasFlaxseed,
                HasBerries = hasBerry,
                HasGreens = hasGreens,
                HasCruciferousVegetables = hasCruciferousVegetables
            }
        });
    }

    public static bool IsGreen(Ingredient ingredient)
    {
        return Greens.IsMatch(ingredient.Name);
    }

    public static bool IsBerry(Ingredient ingredient)
    {
        var isFruit = ingredient.NutritionData?.GetFoodCategoryDescription().Equals(StandardReferenceNutritionData.FruitsAndFruitJuices) ?? false;
        var isBerry = Berries.IsMatch(ingredient.Name);
        return isFruit && isBerry;
    }

    public static bool IsFruit(Ingredient ingredient)
    {
        var isFruit = ingredient.NutritionData?.GetFoodCategoryDescription().Equals(StandardReferenceNutritionData.FruitsAndFruitJuices) ?? false;
        var isBerry = IsBerry(ingredient);
        return isFruit && !isBerry;
    }

    public static bool IsFlaxseed(Ingredient ingredient)
    {
        var isSeed = ingredient.NutritionData?.GetFoodCategoryDescription().Equals(StandardReferenceNutritionData.NutAndSeedProducts) ?? false;
        var IsFlaxseed = ingredient.Name.Contains("flaxseed", StringComparison.InvariantCultureIgnoreCase);
        return isSeed && IsFlaxseed;
    }

    public static bool IsSpicesAndHerbs(Ingredient ingredient)
    {
        var isSpice = ingredient.NutritionData?.GetFoodCategoryDescription().Equals(StandardReferenceNutritionData.SpicesAndHerbs) ?? false;
        var isSalt = ingredient.Name.Contains("salt", StringComparison.InvariantCultureIgnoreCase);
        return isSpice && !isSalt;
    }

    public static bool IsNutsAndSeeds(Ingredient ingredient)
    {
        var isSeed = ingredient.NutritionData?.GetFoodCategoryDescription().Equals(StandardReferenceNutritionData.NutAndSeedProducts) ?? false;
        var isFlaxseed = IsFlaxseed(ingredient);
        return isSeed && !isFlaxseed;
    }


    public static bool IsCruciferousVegetable(Ingredient ingredient)
    {
        return Crucifers.IsMatch(ingredient.Name);
    }
}
