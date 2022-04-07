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

    public async Task OnGetAsync(
        [FromQuery] string search)
    {
        if (search != null)
        {
            await this.Search(search);
        }
        else
        {
            await AllRecipes();
        }
    }

    private async Task Search(string search)
    {
        var recipes = await this._context.SearchRecipes(search)
            .AsSplitQuery()
            .Include(r => r.Images)
            .Include(r => r.Categories)
            .Select(r => new
            {
                Id = r.Id,
                Name = r.Name,
                Categories = r.Categories.Select(c => c.Name),
                Images = r.Images.Select(image => new
                {
                    Id = image.Id,
                    Name = image.Name,
                })
            })
            .OrderBy(r => r.Name)
            .ToListAsync();
        this.Recipes = recipes
            .Select(r =>
                new RecipeView(r.Name, r.Id, r.Images.Select(image => image.Id).ToList(), r.Categories.ToList()))
                .ToList();
    }

    private async Task AllRecipes()
    {
        var complexQueryResults = await _context
            .MultiPartRecipes
            .AsSplitQuery()
            .Include(r => r.Images)
            .Include(r => r.Categories)
            .Select(r => new
            {
                Id = r.Id,
                Name = r.Name,
                Categories = r.Categories.Select(c => c.Name),
                Images = r.Images.Select(image => new
                {
                    Id = image.Id,
                    Name = image.Name,
                })
            })
            .OrderBy(r => r.Name)
            .ToListAsync();
        var simpleQueryResults = await _context
            .Recipes
            .AsSplitQuery()
            .Include(r => r.Images)
            .Include(r => r.Categories)
            .Select(r => new
            {
                Id = r.Id,
                Name = r.Name,
                Categories = r.Categories.Select(c => c.Name),
                Images = r.Images.Select(image => new
                {
                    Id = image.Id,
                    Name = image.Name,
                })
            })
            .OrderBy(r => r.Name)
            .ToListAsync();

        simpleQueryResults.AddRange(complexQueryResults);
        this.Recipes = simpleQueryResults
            .Select(r =>
                new RecipeView(r.Name, r.Id, r.Images.Select(image => image.Id).ToList(), r.Categories.ToList()))
                .ToList();
    }

    public async Task<ActionResult> OnPostAddToCart(Guid recipeId)
    {
        await CartController.AddRecipeToCart(this._context, recipeId);
        return this.RedirectToPage("/Cart");
    }
}

public record RecipeView(string Name, Guid Id, List<Guid> ImageIds, List<string> Categories) {}