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

    private string username;
    private string password;

    private readonly ILogger logger;
    private readonly ApplicationDbContext applicationDbContext;

    public IEmailSender EmailSender { get; }

    public UserManager<ApplicationUser> UserManager { get; }

    // public IQueryable<ApplicationUser> Users { get; } => 

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
        _logger.LogInformation($"Validating user [{userName}]");

        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }
        if (userName.Equals(this.username) && password.Equals(this.password))
        {
            return true;
        }

        return false;
    }

    public IQueryable<ApplicationUser> Users => this.UserManager.Users;

    public IList<Role> GetRoles(ApplicationUser user)
    {
        return this.GetRolesAsync(user).Result
            .Where(role => Enum.TryParse<Role>(role, true, out _))
            .Select(role => Enum.Parse(typeof(Role), role))
            .Cast<Role>()
            .ToList();
    }

    public async Task<IEnumerable<string>> GetRolesAsync(ApplicationUser user) =>
        await this.UserManager.GetRolesAsync(user);

    public ApplicationUser GetUser(ClaimsPrincipal claimsPrincipal)
    {
        var u = this.GetUserAsync(claimsPrincipal).Result;
        if (u == null)
        {
            return null;
        }

        return this.Users
            .Include(us => us.Events)
            .First(user => user.Id == u.Id);
    }

    public async Task<ApplicationUser> GetUserAsync(ClaimsPrincipal claimsPrincipal) =>
        await this.UserManager.GetUserAsync(claimsPrincipal);

    public async Task<ApplicationUser> FindUser(string email)
    {
        return await this.FindByEmailAsync(email);
    }

    public async Task<ApplicationUser> FindByEmailAsync(string email) =>
        await this.UserManager.FindByEmailAsync(email);

    public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string v)
    {
        var result = await this.UserManager.AddToRoleAsync(user, v);
        this.Logger.LogInformation("Result of adding user {User} to tole {Role}: {Result}", user.Name, v, result);
        return result;
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
    {
        return await this.UserManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user) =>
        await this.UserManager.GeneratePasswordResetTokenAsync(user);

    public async Task<ApplicationUser> FindByIdAsync(string id) =>
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

    public async Task<IdentityResult> CreateAsync(
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

    public async System.Threading.Tasks.Task SendEmailVerificationEmailAsync(
        ApplicationUser user,
        Func<string, string> callback)
    {
        var token = await this.GenerateEmailConfirmationTokenAsync(user);
        var url = callback(token);
        await this.EmailSender.SendEmailAsync(
            user.Email,
            "Confirm your CookTime email",
            $"Hello! Please confirm your CookTime account by <a href='{HtmlEncoder.Default.Encode(url)}'>clicking here</a>.");
    }

    public async System.Threading.Tasks.Task SendPasswordResetEmailAsync(
        ApplicationUser user,
        Func<string, string> callback)
    {
        var token = await this.GeneratePasswordResetTokenAsync(user);
        var url = callback(token);
        await this.EmailSender.SendEmailAsync(
            user.Email,
            "Reset your password",
            $"Hello! Please <a href='{HtmlEncoder.Default.Encode(url)}'>click here</a> to reset your Protabla password.");
    }

    public async Task<(IdentityResult, ApplicationUser)> CreateStandardUser(UserSignUp registrationData)
    {
        var user = new ApplicationUser
        {
            Name = registrationData.Name,
            UserName = registrationData.Email,
            Email = registrationData.Email,
        };
        var toReturn = await this.CreateAsync(user, registrationData.Password);
        if (toReturn.Succeeded)
        {
            this.Logger.LogInformation("Created user with email {Email}", user.Email);
            toReturn = await this.AddToRoleAsync(user, Role.StandardUser.ToString());
            if (toReturn.Succeeded)
            {
                var student = new StandardUser()
                {
                    User = user,
                };
                user.StandardUser = student;
                user.EmailConfirmed = registrationData.BypassEmailVerification;
                this.applicationDbContext.StandardUsers.Add(student);
                await this.applicationDbContext.SaveChangesAsync();
                await this.UpdateAsync(user);
                this.Logger.LogInformation("Created student");
            }
        }

        return (toReturn, user);
    }

    public async Task<(IdentityResult, ApplicationUser)> CreateAdmin(string password)
    {
        var user = new ApplicationUser
        {
            Name = "Admin",
            UserName = "Admin@letscooktime.com",
            Email = "Admin@letscooktime.com",
        };
        var toReturn = await this.CreateAsync(user, password);
        if (toReturn.Succeeded)
        {
            this.Logger.LogInformation("Created admin user with email {Email}", user.Email);
            toReturn = await this.AddToRoleAsync(user, Role.SiteAdministrator.ToString());
            user.EmailConfirmed = true;
            await this.UpdateAsync(user);
        }

        return (toReturn, user);
    }

    public async Task UpdateEmailSentTimestampAsync(string email)
    {
        var user = await this.FindUser(email);
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
}