namespace GustavoTech.Implementation;

/// <summary>
/// A UserDetails is a view of a user that we pass to the frontend
/// It's useful when a use wants to GET their own details.
/// </summary>
public record class UserDetails(
    string name,
    string id,
    string[] roles
);