using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms.Pages.Recipes;
[BasicAuth]
public class DeleteModel : PageModel
{
    private readonly babe_algorithms.Services.ApplicationDbContext _context;

    public DeleteModel(babe_algorithms.Services.ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Recipe Recipe { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Recipe = await _context.Recipes.FirstOrDefaultAsync(m => m.Id == id);

        if (Recipe == null)
        {
            return NotFound();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Recipe = await _context.Recipes.FindAsync(id);

        if (Recipe != null)
        {
            _context.Recipes.Remove(Recipe);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
