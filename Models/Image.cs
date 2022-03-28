namespace babe_algorithms.Models;
public class Image : IEquatable<Image>
{
    public Guid Id { get; set; }
    public DateTimeOffset LastModifiedAt { get; set; }
    public string Name { get; set; }
    public byte[] Data { get; set; }

    public bool Equals(Image other) =>
        this.Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}