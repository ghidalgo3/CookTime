namespace babe_algorithms.Controllers;

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using babe_algorithms.Models.Users;
using babe_algorithms.Pages;
using babe_algorithms.Services;
using Microsoft.AspNetCore.Identity;
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

    public AccountController(
        IUserService userManager,
        ISignInManager signInManager,
        ILogger<AccountController> logger,
        ApplicationDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.logger = logger;
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

    [HttpGet("signout")]
    public async Task<IActionResult> Signout()
    {
        var user = this.userManager.GetUser(this.User);
        await this.signInManager.SignOutAsync();
        this.logger.LogInformation("User {UserId} ({UserName}) signed out", user?.Id, user?.UserName);
        return this.Redirect("/Index");
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

                    return this.Ok(new {
                        name = user.UserName,
                        roles = userManager.GetRoles(user).Select(r => r.ToString()),
                    });
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
