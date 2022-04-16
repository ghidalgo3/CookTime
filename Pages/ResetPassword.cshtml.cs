using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace babe_algorithms.Pages;

public enum AlertMessage
{
    None,
    Invalid,
    NeedsValidation,
    Sent,
    PasswordChanged,
    PasswordChangedFailed
}

public class ResetPasswordModel : PageModel
{
    public ResetPasswordModel(IUserService userService, ILogger<ResetPasswordModel> logger)
    {
        this.UserService = userService;
        this.Logger = logger;
    }

    public IUserService UserService { get; }
    public ILogger<ResetPasswordModel> Logger { get; }

    public const string Alert = "AlertMessage";

    [BindProperty]
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [BindProperty]
    // [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 10)]
    [PasswordComplexity(false, false, false, false)]
    public string Password { get; set; }

    [BindProperty]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    [BindProperty]
    public string Token { get; set; }

    [BindProperty]
    public string UserId { get; set; }

    public AlertMessage AlertType { get; set; } = AlertMessage.None;

    public bool HideEmail { get; set; }

    public string Name { get; set; }

    public async Task<IActionResult> OnGet(
        [FromQuery] string token,
        [FromQuery] string userId)
        {
            this.Token = token;
            this.UserId = userId;
            this.Logger.LogInformation("UserId {UserId} started password reset", userId);
            this.Name = (await this.UserService.FindByIdAsync(userId))?.UserName;
            this.HideEmail = this.TempData["HideEmail"] != null;
            if (this.TempData[Alert] is int value)
            {
                this.AlertType = (AlertMessage)value;
            }
            return this.Page();
        }

    public async Task<IActionResult> OnPost()
    {
        if (!this.ModelState.IsValid)
        {
            this.TempData[Alert] = AlertMessage.Invalid;
            return this.RedirectToPage();
        }

        var user = await this.UserService.FindUserByEmail(this.Email);
        if (user == null)
        {
            this.TempData[Alert] = AlertMessage.Invalid;
        }
        else if (!user.EmailConfirmed)
        {
            this.TempData[Alert] = AlertMessage.NeedsValidation;
        }
        else
        {
            this.Logger.LogInformation("Sending password reset email to {Email}", user.Email);
            await this.UserService.SendPasswordResetEmailAsync(user, token => {
                return $"{this.Request.Scheme}://{this.Request.Host.ToUriComponent()}/ResetPassword?token={WebUtility.UrlEncode(token)}&userId={user.Id}#password";
            });
            this.TempData[Alert] = AlertMessage.Sent;
        }
        return this.RedirectToPage();
    }
    public async Task<IActionResult> OnPostPassword()
    {
        this.HideEmail = true;
        this.TempData["HideEmail"] = true;
        var user = await this.UserService.FindByIdAsync(this.UserId);
        if (user == null)
        {
            this.TempData[Alert] = AlertMessage.Invalid;
            return this.RedirectToPage();
        }
        var result = await this.UserService.ResetPasswordAsync(user, this.Token, this.Password);
        if (result.Succeeded)
        {
            this.Logger.LogInformation("User {Email} successfully reset their password", user.Email);
            this.TempData[Alert] = AlertMessage.PasswordChanged;
        }
        else
        {
            this.Logger.LogInformation("User {Email} was not able to change their password", user.Email);
            this.TempData[Alert] = AlertMessage.PasswordChangedFailed;
        }
        return this.RedirectToPage();
    }
}