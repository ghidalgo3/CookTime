using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace babe_algorithms.Pages.Admin;

public class IngredientsViewModel : PageModel
{
    private readonly Services.ApplicationDbContext _context;

    public ISessionManager Session { get; }
    public IUserService UserService { get; }

    public IngredientsViewModel(
        Services.ApplicationDbContext context,
        ISessionManager sessionManager,
        IUserService userService)
    {
        _context = context;
        this.Session = sessionManager;
        this.UserService = userService;
    }

    public List<Ingredient> Ingredients { get; private set; }

    public async Task<IActionResult> OnGet()
    {
        var user = await this.Session.GetSignedInUserAsync(this.User);
        if (user == null)
        {
            return this.RedirectToPage("SignIn");
        }
        this.Ingredients = await this._context.GetIngredients();
        this.Ingredients.Sort((a, b) => string.Compare(a.Name, b.Name));
        return Page();
    }

    public async Task<ActionResult> OnPostUpdateIngredientNdbNumber(
        Guid ingredientId,
        int ndbNumber,
        string gtinUpc,
        string countRegex)
    {

        var user = await this.Session.GetSignedInUserAsync(this.User);
        if (user == null)
        {
            return this.RedirectToPage("SignIn");
        }

        if (!this.UserService.GetRoles(user).Contains(Models.Users.Role.Administrator))
        {
            return this.Page();
        }

        var ingredient = this._context.GetIngredient(ingredientId);
        var nutrition = await this._context.SRNutritionData.FindAsync(ndbNumber);
        var brandedNutrition = await this._context.BrandedNutritionData.FindAsync(gtinUpc);
        if (ingredient == null && (nutrition == null || brandedNutrition == null))
        {
            return this.RedirectToPage();
        }
        ingredient.NutritionData = nutrition;
        ingredient.BrandedNutritionData = brandedNutrition;
        if (nutrition != null)
        {
            nutrition.CountRegex = countRegex;
        }
        await this._context.SaveChangesAsync();
        return this.RedirectToPage();
    }
}
