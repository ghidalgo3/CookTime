using System.Security.Claims;
using babe_algorithms.Models.Users;
using Microsoft.AspNetCore.Identity;

public interface ISignInManager
{
    bool IsSignedIn(ClaimsPrincipal user);

    Task SignInAsync(ApplicationUser user, bool isPersistent);

    Task<SignInResult> PasswordSignInAsync(
        string userName,
        string password,
        bool isPersistent,
        bool lockoutOnFailure);
}