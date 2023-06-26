using System.Security.Claims;
using System.Text.Encodings.Web;
using babe_algorithms.Models.Users;
using babe_algorithms.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace GustavoTech.Implementation;

#nullable enable

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;

    private readonly string? username;
    private readonly string? password;

    private readonly ILogger logger;
    private readonly ApplicationDbContext applicationDbContext;

    public IEmailSender EmailSender { get; }

    public UserManager<ApplicationUser> UserManager { get; }

    protected ILogger Logger { get => this.logger; }

    // inject database for user validation
    public UserService(
        ILogger<UserService> logger,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ApplicationDbContext applicationDbContext,
        IEmailSender emailSender)
    {
        _logger = logger;
        this.username = configuration["Authentication:BasicUsername"];
        this.password = configuration["Authentication:BasicPassword"];
        this.applicationDbContext = applicationDbContext;
        this.EmailSender = emailSender;
        this.UserManager = userManager;
        this.logger = logger;
    }

    public bool IsValidBasicAuthUser(string userName, string password)
    {
        var result = false;
        if (string.IsNullOrWhiteSpace(userName))
        {
            result = false;;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            result = false;;
        }
        if (userName.Equals(this.username) && password.Equals(this.password))
        {
            result = true;
        }

        _logger.LogInformation(
            "Validating user with basic authentication [{userName}]. Authenticated? {result}",
            userName,
            result);
        return result;
    }

    public IQueryable<ApplicationUser> Users => this.UserManager.Users;

    public List<Role> GetRoles(ApplicationUser user)
    {
        return this.GetRolesAsync(user).Result
            .Where(role => Enum.TryParse<Role>(role, true, out _))
            .Select(role => Enum.Parse(typeof(Role), role))
            .Cast<Role>()
            .ToList();
    }

    public async Task<IEnumerable<string>> GetRolesAsync(ApplicationUser user) =>
        await this.UserManager.GetRolesAsync(user);

    public ApplicationUser? GetUser(ClaimsPrincipal claimsPrincipal)
    {
        return this.GetUserAsync(claimsPrincipal).Result;
    }

    public async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal claimsPrincipal) =>
        await this.UserManager.GetUserAsync(claimsPrincipal);

    public async Task<ApplicationUser?> FindUserByEmail(string email)
    {
        return await this.FindByEmailAsync(email);
    }

    public async Task<ApplicationUser?> FindUserByUserName(string userName)
    {
        return await this.UserManager.FindByNameAsync(userName);
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email) =>
        await this.UserManager.FindByEmailAsync(email);

    public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
    {
        var result = await this.UserManager.AddToRoleAsync(user, role);
        this.Logger.LogInformation(
            "Result of adding user {User} to tole {Role}: {Result}", 
            user.UserName,
            role,
            result);
        return result;
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
    {
        return await this.UserManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user) =>
        await this.UserManager.GeneratePasswordResetTokenAsync(user);

    public async Task<ApplicationUser?> FindByIdAsync(string id) =>
        await this.UserManager.FindByIdAsync(id);

    public async Task<IdentityResult> ConfirmEmailAsync(
        ApplicationUser user,
        string verificationCode) =>
        await this.UserManager.ConfirmEmailAsync(user, verificationCode);

    public async Task<IdentityResult> ResetPasswordAsync(
        ApplicationUser user,
        string resetToken,
        string newPassword) =>
        await this.UserManager.ResetPasswordAsync(
            user,
            resetToken,
            newPassword);

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user) =>
        await this.UserManager.UpdateAsync(user);

    private async Task<IdentityResult> CreateAsync(
        ApplicationUser user,
        string? password = null)
    {
            user.LastEmailedDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(password))
            {
                return await this.UserManager.CreateAsync(user, password);
            }
            else
            {
                return await this.UserManager.CreateAsync(user);
            }
        }

    public async Task SendEmailVerificationEmailAsync(
        ApplicationUser user,
        Func<string, string> callback)
    {
        var token = await this.GenerateEmailConfirmationTokenAsync(user);
        var url = callback(token);
        if (user.Email != null)
        {
            await this.EmailSender.SendEmailAsync(
                user.Email,
                "Confirm your CookTime email",
                $"Hello! Please confirm your CookTime account by <a href='{HtmlEncoder.Default.Encode(url)}'>clicking here</a>.");
        }
    }

    public async System.Threading.Tasks.Task SendPasswordResetEmailAsync(
        ApplicationUser user,
        Func<string, string> callback)
    {
        var token = await this.GeneratePasswordResetTokenAsync(user);
        var url = callback(token);

        if (user.Email != null)
        {
            await this.EmailSender.SendEmailAsync(
                user.Email,
                subject: "CookTime password reset",
                htmlMessage: $"Hello! Please <a href='{HtmlEncoder.Default.Encode(url)}'>click here</a> to reset your CookTime password.");
        }
    }

    public async Task<(IdentityResult, ApplicationUser)> CreateUser(UserSignUp registrationData)
    {
        var user = new ApplicationUser
        {
            // Name = registrationData.Name,
            UserName = registrationData.UserName,
            Email = registrationData.Email,
        };
        var toReturn = await this.CreateAsync(user, registrationData.Password);
        if (toReturn.Succeeded)
        {
            this.Logger.LogInformation("Created user {userName} with email {email}", user.UserName, user.Email);
            await this.AddToRoleAsync(user, Role.User.ToString());
        }

        return (toReturn, user);
    }

    public async Task<(IdentityResult, ApplicationUser)> CreateAdmin(string password)
    {
        var user = new ApplicationUser
        {
            UserName = "Admin",
            Email = "Admin@letscooktime.com",
        };
        var toReturn = await this.CreateAsync(user, password);
        if (toReturn.Succeeded)
        {
            this.Logger.LogInformation("Created admin user with email {Email}", user.Email);
            toReturn = await this.AddToRoleAsync(user, Role.Administrator.ToString());
            user.EmailConfirmed = true;
            await this.UpdateAsync(user);
        }

        return (toReturn, user);
    }

    public async Task UpdateEmailSentTimestampAsync(string email)
    {
        var user = await this.FindUserByEmail(email);
        if (user != null)
        {
            user.LastEmailedDate = DateTime.UtcNow;
            await this.applicationDbContext.SaveChangesAsync();
        }
    }

    public async Task<IdentityResult> SetPassword(ApplicationUser user, string newPassword)
    {
        var token = await this.UserManager.GeneratePasswordResetTokenAsync(user);
        return await this.UserManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task<UserDetails> GetUserDetails(ApplicationUser user)
    {
        var roles = await this.GetRolesAsync(user);
        return new(user.UserName, user.Id, roles.Select(role => role.ToString()).ToArray());
    }
}