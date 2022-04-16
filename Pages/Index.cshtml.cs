using System.Diagnostics.CodeAnalysis;
using babe_algorithms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms.Pages.Recipes;

public class IndexModel : PageModel
{
    public ISignInManager SigninManager { get; }

    private readonly ApplicationDbContext _context;

    public IndexModel(
        ISignInManager signinManager,
        ApplicationDbContext context)
    {
        this.SigninManager = signinManager;
        _context = context;
    }

    public List<RecipeView> Recipes { get; set; }

    public async Task OnGetAsync(
        [FromQuery] string search)
    {
        // var signedIn = this.SigninManager.IsSignedIn(this.User);
        if (search != null)
        {
            await this.Search(search);
        }
        else
        {
            await AllRecipes();
        }
    }

    private async Task<List<RecipeView>> GetRecipeViewsForQuery(IQueryable<MultiPartRecipe> query)
    {
        var recipes =
            await query
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
        return recipes
            .Select(r =>
                new RecipeView(r.Name, r.Id, r.Images.Select(image => image.Id).ToList(), r.Categories.ToList()))
                .ToList();
    }

    private async Task Search(string search)
    {
        var byName = await GetRecipeViewsForQuery(this._context.SearchRecipesByName(search));
        byName.AddRange(await GetRecipeViewsForQuery(this._context.GetRecipesWithIngredient(search)));
        byName.AddRange(await GetRecipeViewsForQuery(this._context.SearchRecipesByTag(search)));
        var x = new HashSet<RecipeView>(byName, new RecipeViewEqualityComparer());
        this.Recipes = x.ToList();
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

public class RecipeViewEqualityComparer : IEqualityComparer<RecipeView>
{
    public bool Equals(RecipeView x, RecipeView y) =>
        x.Id == y.Id;

    public int GetHashCode([DisallowNull] RecipeView obj) => 
        obj.Id.GetHashCode();
}