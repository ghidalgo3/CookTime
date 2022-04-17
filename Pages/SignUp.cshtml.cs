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
            var (result, foundUser) = await this.UserService.CreateUser(this.SignUp);
            if (result.Succeeded)
            {
                await SendVerificationEmailAsync(foundUser);
                return this.Redirect("/SignUp#success");
            }
            else
            {
                return this.Redirect("/SignUp#tryagain");
            }
        }
        else if (this.Resend)
        {
            await SendVerificationEmailAsync(user);
            return this.Redirect("/SignUp#success");
        }
        else if (!user.EmailConfirmed)
        {
            return this.Redirect("/SignUp#resendemail");
        }
        else if (user.EmailConfirmed && user.PasswordHash != null)
        {
            return this.Redirect("/SignUp#resendemail");
        }
        else if (user.EmailConfirmed && user.PasswordHash == null)
        {
            return this.Redirect("/SignUp#resendemail");
        }
        else
        {
            return this.Redirect("/SignUp#resendemail");
        }
    }

    private async Task SendVerificationEmailAsync(ApplicationUser foundUser)
    {
        await this.UserService.SendEmailVerificationEmailAsync(
                            foundUser,
                            token =>
                            {
                                // https://localhost:5001/api/Account?userId=a66db18b-de85-40de-a487-e0263c0afad9&confirmationToken=CfDJ8BMwxxpsGSxOkVoQyt82jhbbTgxUrc6QZc83ee0UZnYcNbJSMmaIoqXPiN8ig0r4OhhR2c%2Fv8QA%2BEClAst%2F%2BV%2BBY4bCERfVjFjN2k0LhM4qRJOLbbayhE0HcvX6yPjC%2BzCu4kB2jhWVRPX3A85hbgplDJQ8F9log%2FMff0ggRjzmhmYXtPhTVrICvzzPIchbgPLv6inCw8AYethyGas7Xv7CevR4UbNLCdz4IfAkpv%2BtQxZdxSEBpztmiBsOzx7QE8Q%3D%3D
                                return this.Url.Action(
                                    action: "ConfirmEmail",
                                    controller: "Account",
                                    values: new
                                    {
                                        userId = foundUser.Id,
                                        confirmationToken = token,
                                    },
                                    protocol: this.Request.Scheme);
                            });
    }
}