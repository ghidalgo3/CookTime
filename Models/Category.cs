namespace babe_algorithms.Models;

public class Category
{

    public static readonly List<string> DefaultCategories = new()
    {
                        "Breakfast",
                        "Lunch",
                        "Dinner",
                        "Snack",
                        "Appetizer",
                        "Side dish",
                        "Main dish",
                        "Dessert",
                        "Drink",
                        "Alcohol",
                        "Brunch",
                        "Plant-based",
                        "Vegetarian",
                        "Salad",
                        "Soup",
                        "Baked goods",
                        "Holiday",
                        "Brazilian",
                        "Indian",
                        "Venezuelan",
                        "Korean",
                        "French",
                        "Sauce",
                        "Seasoning"
    };

    [Required]
    public string Name { get; set; }
    public Guid Id { get; set; }
    [Newtonsoft.Json.JsonIgnore]
    public ICollection<Recipe>? Recipes { get; set; }
    [JsonIgnore]
    public ICollection<MultiPartRecipe>? MultiPartRecipes { get; set; }
}