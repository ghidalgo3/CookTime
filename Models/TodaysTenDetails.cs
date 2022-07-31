
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
