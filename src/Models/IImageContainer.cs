using Image = babe_algorithms.Models.Image;

namespace babe_algorithms;

public interface IImageContainer
{
    List<Image> Images { get; set; }
}