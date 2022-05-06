using System.Diagnostics.CodeAnalysis;
using babe_algorithms.Models.Users;
using babe_algorithms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms.Pages.Recipes;

public class FavoritesModel : PageModel
{
    public ISignInManager SigninManager { get; }
    public ISessionManager Session { get; }

    private readonly ApplicationDbContext _context;

    public FavoritesModel(
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

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await this.Session.GetSignedInUserAsync(this.User);
        if (user == null)
        {
            return this.Redirect("/");
        }

        await LoadFavorites(user);
        return this.Page();
    }

    private async Task LoadFavorites(ApplicationUser user)
    {
        var cart = await this._context.GetActiveCartQuery(user, Cart.Favorites).SingleOrDefaultAsync();
        var complexQueryResults = 
            cart.RecipeRequirement
            .Select(rr =>
            {
                var r = rr.MultiPartRecipe;
                return new
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
                    r.ReviewCount,
                };
            })
            .OrderBy(r => r.Name);

        this.Recipes = complexQueryResults
            .Select(r =>
                new RecipeView(
                    r.Name,
                    r.Id,
                    r.Images.Select(image => image.Id).ToList(),
                    r.Categories.ToList(),
                    r.AverageReviews,
                    r.ReviewCount,
                    true
                ))
                .ToList();
    }
}