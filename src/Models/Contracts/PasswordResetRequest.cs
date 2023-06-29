namespace babe_algorithms.Models;

public record PasswordResetRequest
{
    public string Email { get; init; }
}