using babe_algorithms.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace babe_algorithms.Pages;

public class SignUpModel : PageModel
{
    public SignUpModel(
        IUserService userService,
        ISignInManager signinManager)
    {
        this.UserService = userService;
        this.SignInManager = signinManager;
    }

    public IUserService UserService { get; }

    public ISignInManager SignInManager { get; }

    [BindProperty]
    public UserSignUp SignUp { get; set; }

    [BindProperty]
    public bool Resend { get; set; }

    public IActionResult OnGet()
    {
        this.ViewData["BodyClass"] = "signup";
        return this.Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!this.ModelState.IsValid && !this.Resend)
        {
            return this.Page();
        }
        var user = await this.UserService.FindUserByEmail(this.SignUp.Email);

        if (user == null)
        {
            var (result, _) = await this.UserService.CreateUser(this.SignUp);
            if (result.Succeeded)
            {
                // await SendVerificationEmailAsync(foundUser);
                return this.Redirect("/Auth/SignUp#success");
            }
            else
            {
                return this.Redirect("/Auth/SignUp#tryagain");
            }
        }
        else if (this.Resend)
        {
            // await SendVerificationEmailAsync(user);
            return this.Redirect("/Auth/SignUp#success");
        }
        else if (!user.EmailConfirmed)
        {
            return this.Redirect("/Auth/SignUp#resendemail");
        }
        else if (user.EmailConfirmed && user.PasswordHash != null)
        {
            return this.Redirect("/Auth/SignUp#resendemail");
        }
        else if (user.EmailConfirmed && user.PasswordHash == null)
        {
            return this.Redirect("/Auth/SignUp#resendemail");
        }
        else
        {
            return this.Redirect("/Auth/SignUp#resendemail");
        }
    }

}