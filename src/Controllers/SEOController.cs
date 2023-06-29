using babe_algorithms.Services;
using Microsoft.AspNetCore.Mvc;

namespace babe_algorithms;
[Route("api/[controller]")]
[ApiController]

public class SEOController : ControllerBase
{
    private ApplicationDbContext Context { get; init; }

    public SEOController(ApplicationDbContext context)
    {
        this.Context = context;
    }

    [HttpGet("sitemap")]
    public Task GetSiteMap()
    {
        return Task.FromResult(this.Ok());
    }
}