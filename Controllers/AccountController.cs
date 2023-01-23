namespace babe_algorithms.Controllers;

using System.Linq;
using System.Threading.Tasks;
using babe_algorithms.Models.Users;
using babe_algorithms.Pages;
using babe_algorithms.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly ApplicationDbContext appDbContext;
    private readonly IUserService userManager;
    private readonly ISignInManager signInManager;
    private readonly ILogger<AccountController> logger;
    private readonly IAntiforgery antiForgery;

    public AccountController(
        IUserService userManager,
        ISignInManager signInManager,
        ILogger<AccountController> logger,
        ApplicationDbContext appDbContext,
        IAntiforgery antiForgery)
    {
        this.appDbContext = appDbContext;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.logger = logger;
        this.antiForgery = antiForgery;
    }

    [ValidateAntiForgeryToken]
    public void ClearNotifications()
    {
        var user = this.userManager.GetUser(this.User);
        if (user != null)
        {
            var unseenEvents = user.Events.Where(e => (e.Type == EventType.Public) && !e.EventSeen);
            foreach (var e in unseenEvents)
            {
                e.EventSeen = true;
            }

            this.appDbContext.SaveChanges();
        }
    }


    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var user = this.userManager.GetUser(this.User);
        if (user != null)
        {
            return this.Ok(await this.userManager.GetUserDetails(user));
        }
        else
        {
            return this.Unauthorized();
        }
    }

    [HttpGet("signout")]
    public async Task<IActionResult> Signout()
    {
        var user = this.userManager.GetUser(this.User);
        await this.signInManager.SignOutAsync();
        this.logger.LogInformation("User {UserId} ({UserName}) signed out", user?.Id, user?.UserName);
        return this.Redirect("/Index");
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp(
        [FromForm] UserSignUp signUpRequest)
    {
        var user = await this.userManager.FindUserByEmail(signUpRequest.Email);

        if (user == null)
        {
            var (result, foundUser) = await this.userManager.CreateUser(signUpRequest);
            if (result.Succeeded)
            {
                await SendVerificationEmailAsync(foundUser);
                return this.Ok("Email verification needed");
            }
            else
            {
                return this.BadRequest("User could not be created.");
            }
        } else {
            return this.BadRequest("User already exists");
        }
    }

    private async Task SendVerificationEmailAsync(ApplicationUser foundUser)
    {
        await this.userManager
            .SendEmailVerificationEmailAsync(
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

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn(
        [FromForm] SignIn signinRequest)
    {
        try
        {
            if (this.ModelState.IsValid)
            {
                this.logger.LogInformation("Model state is valid, attempting login");
                var user = signinRequest.UserNameOrEmail.Contains('@') ?
                    await this.userManager.FindUserByEmail(signinRequest.UserNameOrEmail) :
                    await this.userManager.FindUserByUserName(signinRequest.UserNameOrEmail);
                var result = await this.signInManager.SignInWithUserName(
                    userName: user.UserName,
                    password: signinRequest.Password,
                    isPersistent: signinRequest.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    this.logger.LogInformation("User {Email} logged in", signinRequest.Email);
                    return this.Ok(await this.userManager.GetUserDetails(user));
                }
                else
                {
                    // string reason = null;
                    // if (user == null)
                    // {
                    //     reason = "user does not exist";
                    //     this.TempData[this.signInFailureTempData] = SignInFailure.IncorrectUsernameOrPassword.ToString();
                    // }
                    // else if (!user.EmailConfirmed)
                    // {
                    //     reason = "email is not confirmed";
                    //     this.TempData[this.signInFailureTempData] = SignInFailure.UnconfirmedEmail.ToString();
                    // }
                    // else if (string.IsNullOrEmpty(user.PasswordHash))
                    // {
                    //     reason = "user does not have a password";
                    //     this.TempData[this.signInFailureTempData] = SignInFailure.NoPassword.ToString();
                    // }
                    // else
                    // {
                    //     this.TempData[this.signInFailureTempData] = SignInFailure.IncorrectUsernameOrPassword.ToString();
                    // }

                    // this.Logger.LogInformation("{Email} failed to log in because {Reason}", signInRequest.Email, reason);

                    // return this.RedirectToPage();
                }
            }
            else
            {
                return this.BadRequest();
            }
        }
        catch
        {
            // this.TempData[this.signInFailureTempData] = SignInFailure.IncorrectUsernameOrPassword.ToString();
        }
        return StatusCode(StatusCodes.Status500InternalServerError);
    }

    // GET api/account/confirmEmail
    [HttpGet("confirmEmail")]
    public async Task<IActionResult> ConfirmEmail(
        string userId,
        string confirmationToken)
    {
        var user = await this.userManager.FindByIdAsync(userId);
        if (user.EmailConfirmed)
        {
            return this.Redirect("/SignIn");
        }
        else
        {
            var result = await this.userManager.ConfirmEmailAsync(user, confirmationToken);
            if (result.Succeeded)
            {
                this.logger.LogInformation("User {UserId} ({UserName}) validated their email", userId, user.UserName);
                await this.signInManager.SignInAsync(user, isPersistent: false);
                var roles = this.userManager.GetRoles(user);
                if (roles.Contains(Role.User))
                {
                    return this.Redirect("/Registration");
                }
                return this.Redirect("/");
            }
            else
            {
                this.logger.LogWarning("User {UserId} failed to validate email", userId);
                return this.Redirect("/SignUp");
            }
        }
    }
}
