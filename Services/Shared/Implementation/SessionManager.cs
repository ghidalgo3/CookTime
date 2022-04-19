using System.Security.Claims;
using babe_algorithms.Models.Users;

#nullable enable

namespace GustavoTech.Implementation;

public class SessionManager : ISessionManager
{

    public SessionManager(IUserService userService, ISignInManager signinManager)
    {
        this.UserService = userService;
        this.SigninManager = signinManager;
    }

    public IUserService UserService { get; }

    public ISignInManager SigninManager { get; }

    public async Task<ApplicationUser?> GetSignedInUserAsync(ClaimsPrincipal cp)
    {

        if (
            this.SigninManager.IsSignedIn(cp)
            && (await this.UserService.GetUserAsync(cp)) is ApplicationUser user)
        {
            return user;
        }
        else
        {
            return null;
        }
    }
}
