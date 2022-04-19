using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using babe_algorithms.Services;

namespace babe_algorithms;
[Route("api/[controller]")]
[ApiController]
public class IngredientController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public IngredientController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Ingredient
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ingredient>>> GetIngredients(
        [FromQuery] string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return await _context.Ingredients.ToListAsync();
        }
        else
        {
            return await _context.Ingredients.Where(ingredient => EF.Functions.Like(ingredient.Name, name)).ToListAsync();
        }
    }

    // GET: api/Ingredient/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Ingredient>> GetIngredient(Guid id)
    {
        var ingredient = await _context.Ingredients.FindAsync(id);

        if (ingredient == null)
        {
            return NotFound();
        }

        return ingredient;
    }

    private bool IngredientExists(Guid id)
    {
        return _context.Ingredients.Any(e => e.Id == id);
    }
}
