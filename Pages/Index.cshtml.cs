using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms.Pages.Recipes;

public class IndexModel : PageModel
{
    private readonly babe_algorithms.Services.ApplicationDbContext _context;

    public IndexModel(babe_algorithms.Services.ApplicationDbContext context)
    {
        _context = context;
    }

    public List<RecipeView> Recipes { get; set; }

    public async Task OnGetAsync()
    {
        var queryResult = await _context
            .Recipes
            .AsSplitQuery()
            .Include(r => r.Images)
            .Select(r => new {
                Id = r.Id,
                Name = r.Name,
                Images = r.Images.Select(image => new 
                {
                    Id = image.Id,
                    Name = image.Name,
                })
            })
            .OrderBy(r => r.Name)
            .ToListAsync();
        this.Recipes = queryResult
            .Select(r =>
                new RecipeView(r.Name, r.Id, r.Images.Select(image => image.Id).ToList()))
                .ToList();
    }

    public async Task<ActionResult> OnPostAddToCart(Guid recipeId)
    {
        var recipe = await _context.GetRecipeAsync(recipeId);
        if (recipe == null)
        {
            return this.Page();
        }

        var cart = await _context.GetActiveCartAsync();
        var existingRecipe = cart.RecipeRequirement.FirstOrDefault(rr => rr.Recipe.Id == recipeId);
        if (existingRecipe == null)
        {
            cart.RecipeRequirement.Add(new RecipeRequirement()
            {
                Recipe = recipe,
                Quantity = 1.0
            });
        }
        else
        {
            existingRecipe.Quantity += 1.0;
        }
        await _context.SaveChangesAsync();
        return this.RedirectToPage("/Cart");
    }
}

public record RecipeView(string Name, Guid Id, List<Guid> ImageIds) {}