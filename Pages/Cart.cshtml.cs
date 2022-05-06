using Microsoft.AspNetCore.Mvc.RazorPages;
using babe_algorithms.Services;
using Microsoft.AspNetCore.Mvc;

namespace babe_algorithms.Pages;
public class CartModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ISessionManager Session { get; }

    public CartModel(
        ApplicationDbContext context,
        ISessionManager sessionManager)
    {
        _context = context;
        this.Session = sessionManager;
    }

    public Models.Cart ActiveCart { get; set; }

    public async Task<ActionResult> OnGetAsync()
    {
        var user = await this.Session.GetSignedInUserAsync(this.User);
        if (user == null)
        {
            return this.RedirectToPage("SignIn");
        }

        ActiveCart = await _context.GetGroceryListAsync(user);
        return this.Page();
    }
}
