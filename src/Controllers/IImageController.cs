using Microsoft.AspNetCore.Mvc;

namespace babe_algorithms;

public interface IImageController
{
    Task<IActionResult> PutImageAsync(Guid containerId, List<IFormFile> files);
    Task<IActionResult> ListImagesAsync(Guid containerId);
    Task<IActionResult> DeleteImageAsync(Guid containerId, Guid imageId);
}