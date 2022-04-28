using babe_algorithms.Models.Users;
using NpgsqlTypes;

namespace babe_algorithms.Models;

public class MultiPartRecipe : IImageContainer, IEquatable<MultiPartRecipe>, IOwned
{
    public MultiPartRecipe() 
    {
    }

    public MultiPartRecipe(Recipe recipe) 
    {
        this.Name = recipe.Name;
        this.StaticImage = recipe.StaticImage;
        this.ServingsProduced = recipe.ServingsProduced;
        this.CooktimeMinutes = (int)recipe.Cooktime.TotalMinutes;
        this.CaloriesPerServing = recipe.CaloriesPerServing;
        // this.Id = recipe.Id;
        this.Categories = recipe.Categories;
        this.Images = recipe.Images;
        this.Source = recipe.Source;
        this.RecipeComponents = new List<RecipeComponent>() 
        {
            new RecipeComponent() 
            {
                Name = recipe.Name,
                Ingredients = recipe.Ingredients.Select(ir => new MultiPartIngredientRequirement(ir)).ToList(),
                Steps = recipe.Steps.Select(s => new MultiPartRecipeStep(s)).ToList(),
            },
        };
    }

    [Required]
    public string Name { get; set; }
    public string? StaticImage { get; set; }
    public double ServingsProduced { get; set; } = 1.0;
    public int CooktimeMinutes { get; set; }
    public double CaloriesPerServing { get; set; }
    public Guid Id { get; set; }
    public ISet<Category> Categories { get; set; }
    public List<Image> Images { get; set; }
    public List<RecipeComponent> RecipeComponents { get; set; } = new List<RecipeComponent>();
    /// <summary>
    /// The source where the recipe came from.
    /// </summary>
    public string? Source { get; set; }

    [JsonIgnore]
    public NpgsqlTsVector SearchVector { get; set; }
    public ApplicationUser? Owner { get; set; }

    public bool Equals(MultiPartRecipe? other) => this.Id == other.Id;
}


