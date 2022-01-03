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
    public Recipe Recipe { get; set; }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.Recipes.Add(Recipe);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Recipes/Details", new { id = this.Recipe.Id });
    }
}
