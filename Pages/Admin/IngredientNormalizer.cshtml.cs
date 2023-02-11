using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms.Pages.Admin;

public class IngredientNormalizerModel : PageModel
{
    private readonly Services.ApplicationDbContext _context;

    public ISessionManager Session { get; }

    public IUserService UserService { get; }

    public Dictionary<Ingredient, int> Frequency = new();

    public IngredientNormalizerModel(
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
        foreach (var ingredient in this.Ingredients)
        {
            this.Frequency[ingredient] =
                this._context.GetRecipesWithIngredient(ingredient.Id).Count();
        }
        this.Ingredients.Sort((a, b) => string.Compare(a.Name, b.Name));
        return Page();
    }

    public async Task<IActionResult> OnPostReplaceIngredient(
        Guid ingredientId,
        Guid replacementId)
    {
        var ingredient = this._context.GetIngredient(replacementId);
        if (ingredient == null)
        {
            return this.NotFound();
        }
        var recipesToModify = await this._context.GetRecipesWithIngredient(ingredientId).ToListAsync();
        foreach (var recipe in recipesToModify)
        {
            recipe.ReplaceIngredient(i => i.Id == ingredientId, ingredient);
        }

        this._context.SaveChanges();
        return this.RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteIngredient(
        Guid ingredientId)
    {
        var ingredient = this._context.GetIngredient(ingredientId);
        if (ingredient == null)
        {
            return this.NotFound();
        }
        var recipesToModify = this._context.GetRecipesWithIngredient(ingredientId).Count();
        if (recipesToModify == 0)
        {
            var mpirs = await this._context.MultiPartIngredientRequirement
                .Include(mpir => mpir.Ingredient)
                .Where(mpir => mpir.Ingredient.Id == ingredient.Id)
                .ToListAsync();
            this._context.MultiPartIngredientRequirement.RemoveRange(mpirs);
            this._context.Ingredients.Remove(ingredient);
        }

        this._context.SaveChanges();
        return this.RedirectToPage();
    }

}
