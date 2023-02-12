namespace babe_algorithms.Pages;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

public class SignInModel : PageModel
{
    private readonly string signInFailureTempData = "SignInFailure";

    public SignInModel(
        ILogger<SignInModel> logger,
        IUserService userManager,
        ISignInManager signInManager)
    {
        this.SignInManager = signInManager;
        this.UserManager = userManager;
        this.Logger = logger;
    }

    private enum SignInFailure
    {
        IncorrectUsernameOrPassword,
        UnconfirmedEmail,
        NoPassword,
    }

    public ISignInManager SignInManager { get; set; }

    public IUserService UserManager { get; set; }

    public ILogger<SignInModel> Logger { get; set; }

    [BindProperty]
    public SignIn SignInData { get; set; }

    public bool IncorrectUsernameOrPassword { get; private set; }

    public bool UnconfirmedEmail { get; set; }

    public bool NoPassword { get; private set; }

    public IActionResult OnGet()
    {
        if (this.SignInManager.IsSignedIn(this.User))
        {
            // If you're already signed in, go home
            var user = this.UserManager.GetUser(this.User);
            if (user == null)
            {
                return this.Redirect("/Index");
            }

            return this.Redirect("/Index");
        }
        else
        {
            if (Enum.TryParse<SignInFailure>(this.TempData[this.signInFailureTempData]?.ToString() ?? string.Empty, out SignInFailure failureMode))
            {
                switch (failureMode)
                {
                    case SignInFailure.IncorrectUsernameOrPassword:
                        this.IncorrectUsernameOrPassword = true;
                        break;
                    case SignInFailure.NoPassword:
                        this.NoPassword = true;
                        break;
                    case SignInFailure.UnconfirmedEmail:
                        this.UnconfirmedEmail = true;
                        break;
                    default:
                        break;
                }
            }

            return this.Page();
        }
    }

    public async Task<IActionResult> OnPostAsync(
        [FromQuery] string? redirectTo)
    {
        try
        {
            if (this.ModelState.IsValid)
            {
                this.Logger.LogInformation("Model state is valid, attempting login");
                var user = this.SignInData.UserNameOrEmail.Contains('@') ?
                    await this.UserManager.FindUserByEmail(this.SignInData.UserNameOrEmail) :
                    await this.UserManager.FindUserByUserName(this.SignInData.UserNameOrEmail);
                var result = await this.SignInManager.SignInWithUserName(
                    userName: user.UserName,
                    password: this.SignInData.Password,
                    isPersistent: this.SignInData.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    this.Logger.LogInformation("User {Email} logged in", this.SignInData.Email);

                    // User does not have an identity yet at this time, and user is null...
                    if (user == null)
                    {
                        return this.Redirect("/Index");
                    }


                    if (redirectTo != null)
                    {
                        redirectTo = System.Web.HttpUtility.UrlDecode(redirectTo);
                        return this.Redirect(redirectTo);
                    }

                    return this.Redirect("/Index");
                }
                else
                {
                    string reason = null;
                    if (user == null)
                    {
                        reason = "user does not exist";
                        this.TempData[this.signInFailureTempData] = SignInFailure.IncorrectUsernameOrPassword.ToString();
                    }
                    else if (!user.EmailConfirmed)
                    {
                        reason = "email is not confirmed";
                        this.TempData[this.signInFailureTempData] = SignInFailure.UnconfirmedEmail.ToString();
                    }
                    else if (string.IsNullOrEmpty(user.PasswordHash))
                    {
                        reason = "user does not have a password";
                        this.TempData[this.signInFailureTempData] = SignInFailure.NoPassword.ToString();
                    }
                    else
                    {
                        this.TempData[this.signInFailureTempData] = SignInFailure.IncorrectUsernameOrPassword.ToString();
                    }

                    this.Logger.LogInformation("{Email} failed to log in because {Reason}", this.SignInData.Email, reason);

                    return this.RedirectToPage();
                }
            }
            else
            {
                return this.Page();
            }
        }
        catch
        {
            this.TempData[this.signInFailureTempData] = SignInFailure.IncorrectUsernameOrPassword.ToString();
        }
        return this.Page();
    }
}

// TODO move to own file
public class SignIn
{
    [EmailAddress]
    public string Email { get; set; }

    public string UserNameOrEmail { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}
