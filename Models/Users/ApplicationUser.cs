using Microsoft.AspNetCore.Identity;

namespace babe_algorithms.Models.Users;

#nullable enable

/// <summary>
/// Base class for identity.
/// All per-user information should be stored as properties of this class.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ApplicationUser : IdentityUser
{
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public EmailFrequency EmailFrequency { get; set; }

    public DateTime LastEmailedDate { get; set; }

    public List<Cart> Carts { get; set; } = new List<Cart>();

    public List<MultiPartRecipe> OwnedRecipes { get; set; } = new List<MultiPartRecipe>();

    [JsonProperty]
    public override string UserName { get; set; }

    [JsonProperty]
    public override string Id { get; set; }
}

public enum EmailFrequency
{
    Daily,
    None,
}