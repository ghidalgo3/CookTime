using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace babe_algorithms.Pages.Recipes;

public class CreateModel : PageModel
{
    private readonly Services.ApplicationDbContext _context;

    public ISessionManager Session { get; }

    public CreateModel(
        Services.ApplicationDbContext context,
        ISessionManager sessionManager)
    {
        _context = context;
        this.Session = sessionManager;
    }

    public async Task<IActionResult> OnGet()
    {
        var user = await this.Session.GetSignedInUserAsync(this.User);
        if (user == null)
        {
            return this.RedirectToPage("/SignIn");
        }
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

        var user = await this.Session.GetSignedInUserAsync(this.User);
        if (user == null)
        {
            return this.RedirectToPage("/SignIn");
        }

        this.Recipe.RecipeComponents.Add(new RecipeComponent()
        {
            Name = this.Recipe.Name,
        });
        this.Recipe.Owner = user;
        this.Recipe.CreationDate = DateTimeOffset.UtcNow;
        this.Recipe.LastModifiedDate = DateTimeOffset.UtcNow;
        _context.MultiPartRecipes.Add(Recipe);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Recipes/Details", new { id = this.Recipe.Id });
    }
}
