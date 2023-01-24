using Microsoft.AspNetCore.Mvc;

namespace babe_algorithms.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    public CategoryController()
    {
    }

    [Route("list")]
    public IActionResult GetCategories()
    {
        return this.Ok(Category.DefaultCategories);
    }
}