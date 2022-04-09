using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace babe_algorithms.Pages.Recipes;

[BasicAuth]
public class CreateModel : PageModel
{
    private readonly babe_algorithms.Services.ApplicationDbContext _context;

    public CreateModel(babe_algorithms.Services.ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    [BindProperty]
    public MultiPartRecipe Recipe { get; set; }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        this.Recipe.RecipeComponents.Add(new RecipeComponent()
        {
            Name = this.Recipe.Name,
        });

        _context.MultiPartRecipes.Add(Recipe);
        await _context.SaveChangesAsync();

        return RedirectToPage($"/Recipes/Details/{this.Recipe.Id}");
    }
}
