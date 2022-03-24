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

    public IActionResult OnGet()
    {
        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
}
