
using System.Text.RegularExpressions;

namespace babe_algorithms.Models;

public partial class TodaysTenDetails 
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

    private static readonly Regex Greens = GreensRegex();

    private static Regex Crucifers = CrucifersRegex();

    private static Regex Berries = BerriesRegex();

    public static DietDetail GetTodaysTenDietDetail(ISet<Ingredient> allIngredients)
    {
        static bool strcmp(string a, string b) => string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);

        bool IsTodaysTen(Ingredient ingredient, string name) =>
            allIngredients.Any(ingredient => strcmp(ingredient.NutritionData?.GetFoodCategoryDescription(), name));

        // Fruit
        var hasFruit = allIngredients.Any(IsFruit);
        // Vegetable
        var hasVegetable = allIngredients.Any(ingredient => IsTodaysTen(ingredient, StandardReferenceNutritionData.VegetableAndVegetableProducts));
        // Bean
        var hasBeans = allIngredients.Any(ingredient => IsTodaysTen(ingredient, StandardReferenceNutritionData.LegumeAndLegumeProducts));
        // Spices
        var hasSpices = allIngredients.Any(IsSpicesAndHerbs);
        // Grain
        var hasGrain = allIngredients.Any(ingredient => IsTodaysTen(ingredient, StandardReferenceNutritionData.CerealGrainsAndPasta));
        // Nuts
        var hasNuts = allIngredients.Any(IsNutsAndSeeds);
        // Flaxseed
        var hasFlaxseed = allIngredients.Any(IsFlaxseed);
        // Berry
        var hasBerry = allIngredients.Any(IsBerry);
        // Greens
        var hasGreens = allIngredients.Any(IsGreen);
        // Crucifers
        var hasCruciferousVegetables = allIngredients.Any(IsCruciferousVegetable);
        
        return new DietDetail()
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
        };
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

    [GeneratedRegex("arugula|greens|beet greens|mustard greens|kale|spring mix|salad|mesclun|collard|sorrel|spinach|swiss chard|turnip greens", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace, "en-US")]
    private static partial Regex GreensRegex();
    [GeneratedRegex("arugula|bok choy|broccoli|brussels sprouts|cabbage|cauliflower|collard greens|horseradish|kale|kohlrabi|mustard greens|radish|turnip greens|watercress", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace, "en-US")]
    private static partial Regex CrucifersRegex();
    [GeneratedRegex("strawberr|blueberr|açai|raspberr|blackberr|cherry", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace, "en-US")]
    private static partial Regex BerriesRegex();
}
