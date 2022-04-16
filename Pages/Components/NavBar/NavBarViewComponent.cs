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

    public string Initials { get; set; }

    public string Name { get; set; }

    public Role Role { get; set; }

    public int UnseenEvents { get; set; }

    public IEnumerable<Event> Events { get; set; }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (this.SignInManager.IsSignedIn(this.UserClaimsPrincipal))
        {
            var user = this.UserManager.GetUser(this.UserClaimsPrincipal);
            if (user == null)
            {
                this.Initials = "UNKNOWN";
                this.Events = new List<Event>();
                return await Task.Run(() => this.View(this));
            }

            var roles = this.UserManager.GetRoles(user);
            if (roles[0] == Role.User)
            {
                this.Name = user.UserName;
                this.Initials = string.Join(string.Empty, this.Name.Split(" ").Select(token => token.Substring(0, 1).ToUpper()) ?? new List<string>());
                this.Role = Role.User;
            }
            else if (roles[0] == Role.Administrator)
            {
                this.Name = user.UserName;
                this.Initials = "ADMIN";
                this.Role = Role.Administrator;
            }

            this.Events = user.Events.Where(e => e.Type == EventType.Public).OrderByDescending(e => e.CreatedAt).Take(10);
            this.UnseenEvents = user.Events.Where(e => e.Type == EventType.Public).Count(e => !e.EventSeen);
        }

        return await Task.Run(() => this.View(this));
    }
}