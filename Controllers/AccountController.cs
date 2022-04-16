namespace babe_algorithms.Controllers;

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using babe_algorithms.Models.Users;
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
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly ILogger<AccountController> logger;

    public AccountController(
        IUserService userManager,
        SignInManager<ApplicationUser> signInManager,
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

    // [ValidateAntiForgeryToken]
    // public void CompleteTutorial()
    // {
    //     var user = this.userManager.GetUser(this.User);
    //     if (user != null)
    //     {
    //         this.appDbContext.SaveChanges();
    //     }
    // }

    [HttpGet("signout")]
    public async Task<IActionResult> Signout()
    {
        var user = this.userManager.GetUser(this.User);
        await this.signInManager.SignOutAsync();
        this.logger.LogInformation("User {UserId} ({UserName}) signed out", user?.Id, user?.UserName);
        return this.Redirect("/Index");
    }

    // [AcceptVerbs("Get", "Post")]
    // public IActionResult ValidateEmailDomain(string email)
    // {
    //     this.logger.LogInformation("Validating {Email}", email);
    //     var domain = Student.GetUniversity(email);
    //     if (domain == null)
    //     {
    //         return this.Json($"{email} is not from a university.");
    //     }
    //     else
    //     {
    //         return this.Json(true);
    //     }
    // }

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

    // private async Task CreateStandardUser(ApplicationUser user)
    // {
    //     var company = new StandardUser()
    //     {
    //         Name = user.Name,
    //         Email = user.Email,
    //         User = user,
    //         SignUp = DateTimeOffset.UtcNow,
    //     };
    //     user.StandardUser = company;
    //     this.appDbContext.StandardUsers.Add(company);
    //     await this.appDbContext.SaveChangesAsync();
    //     await this.userManager.UpdateAsync(user);
    //     this.logger.LogInformation("Created company {CompanyName}", company.Name);
    // }
}
