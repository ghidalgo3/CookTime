using System.Diagnostics;
using babe_algorithms.Models.Users;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NpgsqlTypes;

namespace babe_algorithms.Models;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
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

    public string StaticImage { get; set; }

    public double ServingsProduced { get; set; } = 1.0;

    public int CooktimeMinutes { get; set; }

    public double CaloriesPerServing { get; set; }

    [BindNever]
    public double AverageReviews { get; set; }

    [BindNever]
    public int ReviewCount { get; set; }

    public Guid Id { get; set; }

    public ISet<Category> Categories { get; set; } = new HashSet<Category>();

    public List<Image> Images { get; set; } 

    public List<RecipeComponent> RecipeComponents { get; set; } = new List<RecipeComponent>();

    /// <summary>
    /// The source where the recipe came from.
    /// </summary>
    public string Source { get; set; }

    [JsonIgnore]
    [BindNever]
    public NpgsqlTsVector SearchVector { get; set; }

    public ApplicationUser Owner { get; set; }

    [BindNever]
    public DateTimeOffset CreationDate { get; set; }

    [BindNever]
    public DateTimeOffset LastModifiedDate { get; set; }

    public bool Equals(MultiPartRecipe other) => other != null && this.Id == other.Id;

    public ISet<Ingredient> GetAllIngredients() =>
            this.RecipeComponents
                .SelectMany(ir => ir.Ingredients)
                .Select(ir => ir.Ingredient)
                .ToHashSet();

    public List<string> ApplicableDefaultCategories 
    {
        get
        {
            var applicableCategories = new List<string>();
            var ingredients = this.GetAllIngredients();
            if (ArePlantBased(ingredients))
            {
                applicableCategories.Add("Plant-Based");
            }
            return applicableCategories;
        }
    }

    private static bool ArePlantBased(ISet<Ingredient> ingredients) => ingredients.All(i => i.IsPlantBased);

    public bool ReplaceIngredient(
        Predicate<Ingredient> replace,
        Ingredient replaceWith)
    {
        bool modifiedRecipe = false;
        foreach (var component in this.RecipeComponents)
        {
            foreach (var ingredientRequirement in component.Ingredients)
            {
                if (replace.Invoke(ingredientRequirement.Ingredient))
                {
                    ingredientRequirement.Ingredient = replaceWith;
                    if (string.IsNullOrEmpty(ingredientRequirement.Text))
                    {
                        ingredientRequirement.Text = replaceWith.CanonicalName;
                    }
                    modifiedRecipe = true;
                }
            }
        }
        return modifiedRecipe;
    }

    public override bool Equals(object obj) => Equals(obj as MultiPartRecipe);

    public override int GetHashCode() => this.Id.GetHashCode();

    public override string ToString() => this.Name;

    private string GetDebuggerDisplay() => this.ToString();
}


