using Microsoft.AspNetCore.Identity;

namespace babe_algorithms.Models.Users;

#nullable enable

/// <summary>
/// Base class for identity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }

    public StandardUser StandardUser { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public EmailFrequency EmailFrequency { get; set; }

    public DateTime LastEmailedDate { get; set; }
}

public enum EmailFrequency
{
    Daily,
    None,
}