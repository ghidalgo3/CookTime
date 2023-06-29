using System.Security.Claims;
using babe_algorithms.Models.Users;
#nullable enable
namespace GustavoTech;

public interface ISessionManager
{
    Task<ApplicationUser?> GetSignedInUserAsync(ClaimsPrincipal cp);
    
    bool IsInRole(ApplicationUser user, Role role);
}