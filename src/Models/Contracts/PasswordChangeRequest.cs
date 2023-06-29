namespace babe_algorithms.Models;

public record PasswordChangeRequest
{
    public string Token { get; init; }
    public Guid UserId { get; init; }
    public string Password { get; init; }
    public string ConfirmPassword { get; init; }
}