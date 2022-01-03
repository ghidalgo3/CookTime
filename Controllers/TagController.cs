
using babe_algorithms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace babe_algorithms;

[Route("api/[controller]")]
[ApiController]
public class TagController : ControllerBase
{
    private readonly ApplicationDbContext context;

    public TagController(ApplicationDbContext context)
    {
        this.context = context;
    }

    [HttpGet("list")]
    public async Task<IActionResult> ListTags()
    {
        return this.Ok(await context.Tags.ToListAsync());
    }
}