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

    [HttpGet("internalUpdate")]
    public async Task<ActionResult<List<IngredientInternalUpdate>>> GetInternalIngredientView([FromQuery] string search)
    {
        var ingredients = await this._context.GetIngredients(search);
        var result = ingredients.Select(i => IngredientInternalUpdate.FromIngredient(i)).ToList();
        return this.Ok(result);
    }
    
    [HttpGet("normalized")]
    public async Task<ActionResult<List<IngredientReplacementRequest>>> GetNormalizedIngredientsView([FromQuery] string search)
    {
        Dictionary<Ingredient, int> frequency = new();
        var ingredients = await this._context.GetIngredients(search);
        foreach (var ingredient in ingredients)
        {
            frequency[ingredient] =
                this._context.GetRecipesWithIngredient(ingredient.Id).Count();
        }
        var result = ingredients.Select(i => IngredientReplacementRequest.FromIngredient(i, frequency[i])).ToList();
        return this.Ok(result);
    }

    [HttpPost("replace")]
    public async Task<IActionResult> OnPostReplaceIngredient(IngredientReplacementRequest request)
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
        var ingredient = this._context.GetIngredient(request.ReplacedId);
        if (ingredient == null)
        {
            return this.NotFound();
        }
        var recipesToModify = await this._context.GetRecipesWithIngredient(request.KeptId).ToListAsync();
        foreach (var recipe in recipesToModify)
        {
            recipe.ReplaceIngredient(i => i.Id == request.KeptId, ingredient);
        }

        this._context.SaveChanges();
        return this.Ok();
    }

    [HttpDelete("{ingredientId}")]
    public async Task<IActionResult> OnPostDeleteIngredient(
        Guid ingredientId)
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
        var ingredient = this._context.GetIngredient(ingredientId);
        if (ingredient == null)
        {
            return this.NotFound();
        }
        var recipesToModify = this._context.GetRecipesWithIngredient(ingredientId).Count();
        if (recipesToModify == 0)
        {
            var mpirs = await this._context.MultiPartIngredientRequirement
                .Include(mpir => mpir.Ingredient)
                .Where(mpir => mpir.Ingredient.Id == ingredient.Id)
                .ToListAsync();
            this._context.MultiPartIngredientRequirement.RemoveRange(mpirs);
            this._context.Ingredients.Remove(ingredient);
        }

        this._context.SaveChanges();
        return this.Ok();
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
