using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using babe_algorithms.Services;

namespace babe_algorithms;
[Route("api/[controller]")]
[ApiController]
public class IngredientController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ISessionManager Session { get; }

    public IngredientController(
        ApplicationDbContext context,
        ISessionManager sessionManager)
    {
        _context = context;
        this.Session = sessionManager;
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
            return this.Ok(await this._context.GetIngredientsForAutosuggest(name));
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

    [HttpGet("internalUpdate")]
    public async Task<ActionResult<List<IngredientInternalUpdate>>> GetInternalIngredientView([FromQuery] string search)
    {
        var ingredients = await this._context.GetIngredients(search);
        var result = ingredients.Select(i => IngredientInternalUpdate.FromIngredient(i)).ToList();
        return this.Ok(result);
    }

    [HttpPost("internalUpdate")]
    public async Task<ActionResult> OnPostUpdateIngredientNdbNumber(
        [FromBody]
        IngredientInternalUpdate update)
    {

        var user = await this.Session.GetSignedInUserAsync(this.User);
        if (user == null)
        {
            return this.Unauthorized("You must be an signed in ");
        }

        if (!this.Session.IsInRole(user, Models.Users.Role.Administrator))
        {
            return this.Unauthorized("You must be an administrator");
        }

        var ingredient = this._context.GetIngredient(update.IngredientId);
        var nutrition = await this._context.SRNutritionData.FindAsync(update.NdbNumber);
        var brandedNutrition = await this._context.BrandedNutritionData.FindAsync(update.GtinUpc);
        if (ingredient == null && (nutrition == null || brandedNutrition == null))
        {
            return this.BadRequest("No ingredient or nutrition data found.");
        }
        ingredient.NutritionData = nutrition;
        ingredient.BrandedNutritionData = brandedNutrition;
        ingredient.Name = update.IngredientNames;
        if (nutrition != null)
        {
            nutrition.CountRegex = update.CountRegex;
        }
        ingredient.ExpectedUnitMass = update.ExpectedUnitMass;
        await this._context.SaveChangesAsync();
        await this._context.Entry(ingredient).ReloadAsync();
        return this.Ok(IngredientInternalUpdate.FromIngredient(ingredient));
    }
}
