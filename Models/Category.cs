namespace babe_algorithms.Models;

public class Category
{
    [Required]
    public string Name { get; set; }
    public Guid Id { get; set; }
    [Newtonsoft.Json.JsonIgnore]
    public ICollection<Recipe>? Recipes { get; set; }
    [JsonIgnore]
    public ICollection<MultiPartRecipe>? MultiPartRecipes { get; set; }
}