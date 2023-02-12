using babe_algorithms.Services;
using babe_algorithms.ViewComponents;
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

    // public List<RecipeView> Recipes { get; set; }

    public List<RecipeView> NewRecipes { get; set; } = new List<RecipeView>();

    public List<RecipeView> FeaturedRecipes { get; set; } = new List<RecipeView>();

    public Cart Favorites { get; private set; }

    public PagedResult<object> PagedResult { get; private set; }

    public async Task OnGetAsync(
        [FromQuery]
        int page = 1,
        [FromQuery]
        string search = null)
    {

    }


}