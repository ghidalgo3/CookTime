namespace babe_algorithms.Models.Users;

public interface IOwned
{
    ApplicationUser Owner { get; set; }
}