using System.ComponentModel.DataAnnotations.Schema;
using babe_algorithms.Models;
namespace babe_algorithms;

[Table("MultiPartRecipe")]
public class MultiPartRecipe : IImageContainer
{
    public MultiPartRecipe() 
    {
        
    }
    public MultiPartRecipe(Recipe recipe) 
    {
        this.Name = recipe.Name;
        this.StaticImage = recipe.StaticImage;
        this.ServingsProduced = recipe.ServingsProduced;
        this.Cooktime = recipe.Cooktime;
        this.CaloriesPerServing = recipe.CaloriesPerServing;
        this.Id = recipe.Id;
        this.Categories = recipe.Categories;
        this.Images = recipe.Images;
        this.Source = recipe.Source;
        this.RecipeComponents = new List<RecipeComponent>() 
        {
            new RecipeComponent() 
            {
                Name = recipe.Name,
                Ingredients = recipe.Ingredients,
                Steps = recipe.Steps,
            },
        };
    }

    [Required]
    public string Name { get; set; }
    public string StaticImage { get; set; }
    public double ServingsProduced { get; set; } = 1.0;
    public TimeSpan Cooktime { get; set; }
    public double CaloriesPerServing { get; set; }
    public Guid Id { get; set; }
    public ISet<Category> Categories { get; set; }
    public List<Image> Images { get; set; }
    public List<RecipeComponent> RecipeComponents { get; set; }

    /// <summary>
    /// The source where the recipe came from.
    /// </summary>
    public string Source { get; set; }
}