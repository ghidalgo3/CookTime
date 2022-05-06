using System.Diagnostics.CodeAnalysis;
using babe_algorithms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms.Pages.Recipes;

public class IndexModel : PageModel
{
    public ISignInManager SigninManager { get; }

    public ISessionManager Session { get; }

    private readonly ApplicationDbContext _context;

    public IndexModel(
        ISignInManager signinManager,
        ISessionManager sessionManager,
        ApplicationDbContext context)
    {
        this.SigninManager = signinManager;
        this.Session = sessionManager;
        _context = context;
    }

    public List<RecipeView> Recipes { get; set; }
    public Cart Favorites { get; private set; }

    public async Task OnGetAsync(
        [FromQuery]
        string search)
    {
        var user = await this.Session.GetSignedInUserAsync(this.User);
        if (user != null)
        {
            this.Favorites = await this._context.GetFavoritesAsync(user);
        }

        if (search != null)
        {
            await this.Search(search);
        }
        else
        {
            await AllRecipes();
        }
    }

    public static async Task<List<RecipeView>> GetRecipeViewsForQuery(
        IQueryable<MultiPartRecipe> query,
        Cart? favorites)
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
                    }),
                    r.AverageReviews,
                    r.ReviewCount
                })
                .OrderBy(r => r.Name)
                .ToListAsync();
        return recipes
            .Select(r =>
                new RecipeView(
                    r.Name,
                    r.Id,
                    r.Images.Select(image => image.Id).ToList(),
                    r.Categories.ToList(),
                    r.AverageReviews,
                    r.ReviewCount,
                    favorites?.ContainsRecipe(r.Id) ?? null))
                .ToList();
    }

    private async Task Search(string search)
    {
        var byName = await GetRecipeViewsForQuery(
            this._context.SearchRecipesByName(search),
            this.Favorites);
        byName.AddRange(await GetRecipeViewsForQuery(
            this._context.GetRecipesWithIngredient(search),
            this.Favorites));
        byName.AddRange(await GetRecipeViewsForQuery(
            this._context.SearchRecipesByTag(search),
            this.Favorites));
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
                }),
                r.AverageReviews,
                r.ReviewCount
            })
            .OrderBy(r => r.Name)
            .ToListAsync();

        this.Recipes = complexQueryResults
            .Select(r =>
                new RecipeView(
                    r.Name,
                    r.Id,
                    r.Images.Select(image => image.Id).ToList(),
                    r.Categories.ToList(),
                    r.AverageReviews,
                    r.ReviewCount,
                    this.Favorites?.ContainsRecipe(r.Id) ?? null))
                .ToList();
    }
}

public record RecipeView(
    string Name,
    Guid Id,
    List<Guid> ImageIds,
    List<string> Categories,
    double AverageReviews,
    int ReviewCount,
    bool? IsFavorite) {}

public class RecipeViewEqualityComparer : IEqualityComparer<RecipeView>
{
    public bool Equals(RecipeView x, RecipeView y) =>
        x.Id == y.Id;

    public int GetHashCode([DisallowNull] RecipeView obj) => 
        obj.Id.GetHashCode();
}