using System.Security.Claims;
using babe_algorithms.Models.Users;
using GustavoTech.Implementation;
using Microsoft.AspNetCore.Identity;

namespace GustavoTech;

#nullable enable

/// <summary>
/// Microsoft.AspNetCore.Identity.UserManager<TUser> is the framework-provided
/// user management abstration, but it does not implement an interface
/// and so it is difficult to test. 
/// 
/// This interface wraps the calls we make against UserManager for testability purposes.
/// </summary>
public interface IUserService
{
    bool IsValidBasicAuthUser(string userName, string password);

    Task<(IdentityResult, ApplicationUser)> CreateUser(UserSignUp user);

    Task<ApplicationUser?> FindByIdAsync(string id);

    Task<IdentityResult> SetPassword(ApplicationUser user, string newPassword);

    Task<IdentityResult> ConfirmEmailAsync(
        ApplicationUser user,
        string verificationCode);

    Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);

    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);

    Task<IdentityResult> ResetPasswordAsync(
        ApplicationUser user,
        string resetToken,
        string newPassword);

    /// <summary>
    /// Sends an email verification token. Callers control where the link takes the user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="callback">A function that consumes the token and produces a URI for the user to click on.</param>
    /// <returns>Awaitable task.</returns>
    Task SendEmailVerificationEmailAsync(
        ApplicationUser user,
        Func<string, string> callback);

    /// <summary>
    /// Sends an email with password reset instructions. Callers control where the link takes the user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="callback">A function that consumes the password reset token and produces a URI for the user to click on.</param>
    /// <returns>Awaitable task.</returns>
    Task SendPasswordResetEmailAsync(
        ApplicationUser user,
        Func<string, string> callback);

    Task<ApplicationUser?> FindUserByEmail(string email);

    Task<ApplicationUser?> FindUserByUserName(string email);

    Task UpdateEmailSentTimestampAsync(string email);

    List<Role> GetRoles(ApplicationUser user);

    ApplicationUser? GetUser(ClaimsPrincipal claimsPrincipal);

    Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal claimsPrincipal);

    Task<IdentityResult> UpdateAsync(ApplicationUser user);

    Task<UserDetails> GetUserDetails(ApplicationUser user);
}