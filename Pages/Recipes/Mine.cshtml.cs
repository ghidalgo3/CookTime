using babe_algorithms.Models.Users;
using babe_algorithms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms.Pages.Recipes;

public class MineModel : PageModel
{
    public ISignInManager SigninManager { get; }

    public ISessionManager Session { get; }

    private readonly ApplicationDbContext _context;

    public MineModel(
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

        this.Favorites = await this._context.GetFavoritesAsync(user);
        this.Recipes = await IndexModel.GetRecipeViewsForQuery(
            this._context.MultiPartRecipes.Where(recipe => recipe.Owner.Id == user.Id), this.Favorites);
        return this.Page();
    }
}