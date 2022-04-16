using System.Security.Claims;
using babe_algorithms.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace GustavoTech.Implementation;

public class SignInManager : ISignInManager
{
    private readonly SignInManager<ApplicationUser> signInManager;

    public SignInManager(SignInManager<ApplicationUser> signinManager)
    {
        this.signInManager = signinManager;
    }

    public bool IsSignedIn(ClaimsPrincipal user)
    {
        return this.signInManager.IsSignedIn(user);
    }

    public async Task<SignInResult> SignInWithUserName(
        string userName,
        string password,
        bool isPersistent,
        bool lockoutOnFailure)
    {
        return await this.signInManager.PasswordSignInAsync(
            userName,
            password,
            isPersistent,
            lockoutOnFailure);
    }

    public async Task SignInAsync(ApplicationUser user, bool isPersistent)
    {
        await this.signInManager.SignInAsync(user, isPersistent);
    }
}