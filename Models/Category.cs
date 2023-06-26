namespace babe_algorithms.Models;

public class Category
{

    public static readonly List<string> DefaultCategories = new()
    {
                        "Alcohol",
                        "Appetizer",
                        "Baked goods",
                        "Brazilian",
                        "Breakfast",
                        "Brunch",
                        "Dessert",
                        "Dinner",
                        "Drink",
                        "French",
                        "Holiday",
                        "Indian",
                        "Korean",
                        "Lunch",
                        "Main dish",
                        "Plant-Based",
                        "Salad",
                        "Sauce",
                        "Seasoning",
                        "Side dish",
                        "Snack",
                        "Soup",
                        "Vegetarian",
                        "Venezuelan",
    };

    [Required]
    public string Name { get; set; }

    public Guid Id { get; set; }

    [JsonIgnore]
    public ICollection<Recipe> Recipes { get; set; }

    [JsonIgnore]
    public ICollection<MultiPartRecipe> MultiPartRecipes { get; set; }
}