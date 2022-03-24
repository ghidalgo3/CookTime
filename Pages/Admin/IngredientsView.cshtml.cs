using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace babe_algorithms.Pages.Admin;

[BasicAuth]
public class IngredientsViewModel : PageModel
{
    private readonly babe_algorithms.Services.ApplicationDbContext _context;

    public IngredientsViewModel(babe_algorithms.Services.ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Ingredient> Ingredients { get; private set; }

    public async Task<IActionResult> OnGet()
    {
        this.Ingredients = await this._context.GetIngredients();
        return Page();
    }

    public async Task<ActionResult> OnPostUpdateIngredientNdbNumber(Guid ingredientId, int ndbNumber)
    {
        var ingredient = this._context.GetIngredient(ingredientId);
        var nutrition = await this._context.SRNutritionData.FindAsync(ndbNumber);
        if (ingredient == null || nutrition == null)
        {
            return this.RedirectToPage();
        }
        ingredient.NutritionData = nutrition;
        await this._context.SaveChangesAsync();
        return this.RedirectToPage();
    }
    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
}
