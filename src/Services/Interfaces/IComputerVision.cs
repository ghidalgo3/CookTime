namespace babe_algorithms;

public interface IComputerVision
{
    public Task<string> GetTextAsync(Stream image, CancellationToken ct);
}