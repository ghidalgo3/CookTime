using babe_algorithms.Models.Users;
using babe_algorithms.Services;
using Microsoft.AspNetCore.Mvc;
namespace babe_algorithms.ViewComponents;

public class NavBarViewComponent : ViewComponent
{
    public NavBarViewComponent(
        IUserService userManager,
        ISignInManager signInManager)
    {
        this.SignInManager = signInManager;
        this.UserManager = userManager;
    }

    public ISignInManager SignInManager { get; }

    public IUserService UserManager { get; }

    public string UserName { get; set; }

    public List<Role> Roles { get; set; }

    public int UnseenEvents { get; set; }

    public IEnumerable<Event> Events { get; set; }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (this.SignInManager.IsSignedIn(this.UserClaimsPrincipal))
        {
            var user = this.UserManager.GetUser(this.UserClaimsPrincipal);
            if (user == null)
            {
                this.UserName = "UNKNOWN";
                this.Events = new List<Event>();
                return await Task.Run(() => this.View(this));
            }

            var roles = this.UserManager.GetRoles(user);
            this.UserName = user.UserName;
            this.Roles = roles;
            this.Events = user.Events.Where(e => e.Type == EventType.Public).OrderByDescending(e => e.CreatedAt).Take(10);
            this.UnseenEvents = user.Events.Where(e => e.Type == EventType.Public).Count(e => !e.EventSeen);
        }

        return await Task.Run(() => this.View(this));
    }

    public bool IsAdmin()
    {
        return this.Roles.Contains(Role.Administrator);
    }
}