namespace babe_algorithms.Models;

public record SignUpResult
{
    public bool Success { get; init; } = default;
    public string Message { get; init; }
};