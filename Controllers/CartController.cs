using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using babe_algorithms.Services;
using babe_algorithms.Models.Users;
using GustavoTech.Implementation;

namespace babe_algorithms;

[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ISessionManager Session { get; }

    public CartController(
        ApplicationDbContext context,
        ISessionManager sessionManager)
    {
        _context = context;
        this.Session = sessionManager;
    }

    public static async Task AddRecipeToCart(
        ApplicationDbContext _context,
        ApplicationUser user,
        Guid recipeId)
    {
        var recipe = await _context.GetRecipeAsync(recipeId);
        var mpRecipe = await _context.GetMultiPartRecipeAsync(recipeId);
        if (recipe == null && mpRecipe == null)
        {
            return;
        }

        if (recipe != null)
        {
            var cart = await _context.GetActiveCartAsync(user);
            var existingRecipe = cart.RecipeRequirement.FirstOrDefault(rr => rr.Recipe?.Id == recipeId);
            if (existingRecipe == null)
            {
                cart.RecipeRequirement.Add(new RecipeRequirement()
                {
                    Recipe = recipe,
                    Quantity = 1.0
                });
            }
            else
            {
                existingRecipe.Quantity += 1.0;
            }
            await _context.SaveChangesAsync();
        }
        else
        {
            var cart = await _context.GetActiveCartAsync(user);
            var existingRecipe = cart.RecipeRequirement.FirstOrDefault(rr => rr.MultiPartRecipe.Id == recipeId);
            if (existingRecipe == null)
            {
                cart.RecipeRequirement.Add(new RecipeRequirement()
                {
                    MultiPartRecipe = mpRecipe,
                    Quantity = 1.0
                });
            }
            else
            {
                existingRecipe.Quantity += 1.0;
            }
            await _context.SaveChangesAsync();
        }
    }

    // GET: api/Cart
    [HttpGet]
    public async Task<ActionResult<Cart>> GetCart()
    {
        if ((await this.Session.GetSignedInUserAsync(this.User)) is ApplicationUser user)
        {
            return await _context.GetActiveCartAsync(user);
        }
        else
        {
            return this.Unauthorized();
        }
    }

    // PUT: api/Cart/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCart(
        Guid id,
        Cart cartPayload)
    {
        if (id != cartPayload.Id)
        {
            return BadRequest();
        }
        var user = await this.Session.GetSignedInUserAsync(this.User);
        if (user == null)
        {
            return this.Unauthorized();
        }

        var cart = await this._context.GetActiveCartAsync(user);
        _context.Entry(cart).CurrentValues.SetValues(cartPayload);
        cart.RecipeRequirement = cart.RecipeRequirement
            .Where(rr => cartPayload.RecipeRequirement.Contains(rr))
            .ToList();
        cart.RecipeRequirement.ForEach(rr => _context.Entry(rr).CurrentValues.SetValues(cartPayload.RecipeRequirement.FirstOrDefault(r => r.Id == rr.Id)));
        // keep the ones that haven't changed
        cart.IngredientState = cart.IngredientState.Where(@is => cartPayload.IngredientState.Contains(@is)).ToList();
        // add new ones
        var newIngredientStates = cartPayload.IngredientState.Where(@is => !cart.IngredientState.Contains(@is));
        foreach (var @is in newIngredientStates)
        {
            @is.Ingredient = _context.GetIngredient(@is.Ingredient.Id);
        }
        cart.IngredientState.AddRange(newIngredientStates);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CartExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Cart
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Cart>> PostCart(Guid recipeId)
    {
        if ((await this.Session.GetSignedInUserAsync(this.User)) is ApplicationUser user)
        {
            await CartController.AddRecipeToCart(this._context, user, recipeId);
            return this.Redirect("/Cart");
        }
        else
        {
            return this.Unauthorized();
        }
    }

    // DELETE: api/Cart/clear
    [HttpPost("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var user = await this.Session.GetSignedInUserAsync(this.User);
        if (user == null)
        {
            return this.Unauthorized();
        }

        var cart = await _context.GetActiveCartAsync(user);
        if (cart == null)
        {
            return NotFound();
        }
        cart.RecipeRequirement.Clear();
        cart.IngredientState.Clear();
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private bool CartExists(Guid id)
    {
        return _context.Carts.Any(e => e.Id == id);
    }
}
