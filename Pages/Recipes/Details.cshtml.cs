using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace babe_algorithms.Pages.Recipes;

public class DetailsModel : PageModel
{
    private readonly babe_algorithms.Services.ApplicationDbContext _context;

    public DetailsModel(babe_algorithms.Services.ApplicationDbContext context)
    {
        _context = context;
    }

    public string Name { get; set; }
    public Guid Id { get; set; }
    public bool IsMultipart { get; set; }

    public async Task<IActionResult> OnGetAsync(
        [FromRoute] Guid? recipeId,
        [FromQuery] Guid? id)
    {
        if (id == null && recipeId == null)
        {
            return NotFound();
        }

        var simpleRecipe = await _context.Recipes.FindAsync(recipeId);
        if (simpleRecipe == null)
        {
            var complex = await _context.MultiPartRecipes.FindAsync(recipeId);
            if (complex == null)
            {
                return this.NotFound();
            }
            this.IsMultipart = true;
            this.Name = complex.Name;
            this.Id = complex.Id;
        }
        else
        {
            this.Name = simpleRecipe.Name;
            this.Id = simpleRecipe.Id;
        }

        return Page();
    }
}
