using Microsoft.AspNetCore.Identity;

namespace babe_algorithms.Models.Users;

#nullable enable

/// <summary>
/// Base class for identity.
/// All per-user information should be stored as properties of this class.
/// </summary>
public class ApplicationUser : IdentityUser
{
    [JsonIgnore]
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    [JsonIgnore]
    public EmailFrequency EmailFrequency { get; set; }

    [JsonIgnore]
    public DateTime LastEmailedDate { get; set; }

    [JsonIgnore]
    public List<Cart> Carts { get; set; } = new List<Cart>();

    [JsonIgnore]
    public List<MultiPartRecipe> OwnedRecipes { get; set; } = new List<MultiPartRecipe>();

#nullable disable
    [JsonInclude]
    [Required]
    public override string UserName { get; set; }

    [JsonInclude]
    [Required]
    public override string Id { get; set; }
}

public enum EmailFrequency
{
    Daily,
    None,
}