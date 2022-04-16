using Microsoft.AspNetCore.Identity;

namespace babe_algorithms.Models.Users;

#nullable enable

/// <summary>
/// Base class for identity.
/// All per-user information should be stored as properties of this class.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public EmailFrequency EmailFrequency { get; set; }

    public DateTime LastEmailedDate { get; set; }
}

public enum EmailFrequency
{
    Daily,
    None,
}