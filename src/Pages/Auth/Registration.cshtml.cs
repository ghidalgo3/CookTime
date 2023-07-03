using babe_algorithms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace babe_algorithms.Pages;

public class RegistrationModel : PageModel
{

    [BindProperty]
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 10)]
    [PasswordComplexity(false, false, false, false)]
    public string Password { get; set; }

    [BindProperty]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    public IUserService UserService { get; }
    public ApplicationDbContext AppDbContext { get; }

    public RegistrationModel(
        IUserService userService,
        ApplicationDbContext context)
    {
        this.UserService = userService;
        this.AppDbContext = context;
    }

    public async Task<IActionResult> OnGet()
    {
        var currentUser = await this.UserService.GetUserAsync(this.User);
        if (currentUser == null)
        {
            return this.Redirect("/");
        }
        if (currentUser.PasswordHash == null)
        {
            return this.Page();
        }
        else
        {
            return this.Redirect("/");
        }
    }
}
