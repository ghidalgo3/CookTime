using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using babe_algorithms.Services;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace babe_algorithms;

[Route("api/[controller]")]
[ApiController]
public class RecipeController : ControllerBase, IImageController
{
    private readonly ApplicationDbContext context;

    public ISessionManager Session { get; }

    public RecipeController(
        ApplicationDbContext context,
        ISessionManager sessionManager)
    {
        this.context = context;
        this.Session = sessionManager;
    }

    // GET: api/Recipe
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipes() =>
        await context.Recipes.ToListAsync();

    [HttpGet("units")]
    public ActionResult<IEnumerable<string>> GetUnits()
    {
        var allUnits = Enum.GetValues<Unit>();
        var body = allUnits.Select(unit => 
        {
            string siType = "Count";
            if ((int)unit < 1000)
            {
                siType = "Volume";
            }
            else if ((int)unit >= 2000)
            {
                siType = "Weight";
            }
            return new { Name = unit.ToString(), SIType = siType, siValue = unit.GetSIValue() };
        });
        return this.Ok(body);
    }

    [HttpGet("tags")]
    public ActionResult<IEnumerable<string>> GetTags(string query)
    {
        var result =
            string.IsNullOrEmpty(query) ?
                this.context.Categories.Select(cat => new { cat.Name, cat.Id }).ToList()
            :
            this.context.Categories
                .Where(cat => cat.Name.ToUpper().Contains(query.ToUpper()))
                .Select(cat => new { cat.Name, cat.Id }).ToList();
        return this.Ok(result);
    }

    // GET: api/Recipe/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Recipe>> GetRecipe(Guid id)
    {
        var recipe = await context.GetRecipeAsync(id);

        if (recipe == null)
        {
            return NotFound();
        }

        if (recipe.Ingredients.All(ir => ir.Position == 0))
        {
            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                recipe.Ingredients[i].Position = i;
            }
        }

        return Ok(recipe);
    }


    [HttpGet("ingredients")]
    public async Task<ActionResult<IEnumerable<Ingredient>>> GetIngredients(
        [FromQuery(Name = "name")]
        string name)
    {
        // god shield me from this
        // var initialQuery = await context.Ingredients.Where(i => i.Name.ToUpper().Contains(name.ToUpper())).ToListAsync();
        return this.Ok(await this.context.GetIngredientsForAutosuggest(name));
    }

    // DELETE: api/Recipe/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRecipe(Guid id)
    {
        var user = await this.Session.GetSignedInUserAsync(this.User);
        if (user == null)
        {
            return this.Unauthorized();
        }

        var recipe = await context.GetRecipeAsync(id);
        if (recipe == null)
        {
            return NotFound();
        }

        context.Recipes.Remove(recipe);
        foreach (var image in recipe.Images)
        {
            context.Images.Remove(image);
        }
        var cart = await context.GetGroceryListAsync(user);
        cart.RecipeRequirement = cart.RecipeRequirement.Where(rr => rr.Recipe.Id != id).ToList();
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{containerId}/image")]
    public async Task<IActionResult> PutImageAsync(
        [FromRoute] Guid containerId,
        [FromForm] List<IFormFile> files)
    {
        var recipe = await this.context.Recipes.Include(r => r.Images).FirstAsync(r => r.Id == containerId);
        if (recipe == null)
        {
            return NotFound("recipe");
        }
        if (files.Count != 1)
        {
            return this.BadRequest("One image at a time");
        }

        var file = files[0];
        using var fileStream = file.OpenReadStream();
        var _image = await SixLabors.ImageSharp.Image.LoadAsync(fileStream);
        using var outputStream = new MemoryStream();
        // Now save as Jpeg
        await _image.SaveAsync(outputStream, new JpegEncoder());
        var newId = Guid.NewGuid();
        var image = new Image()
        {
            Id = newId,
            Name = newId.ToString(),
            LastModifiedAt = DateTimeOffset.UtcNow,
            Data = outputStream.ToArray(),
        };
        if (recipe.Images.Count > 0)
        {
            // only allow one image
            var toRemove = recipe.Images[0];
            context.Images.Remove(toRemove);
            recipe.Images.Clear();
        }
        recipe.Images.Add(image);
        context.Images.Add(image);
        await this.context.SaveChangesAsync();
        return this.Ok();
    }

    [HttpGet("{containerId}/images")]
    public async Task<IActionResult> ListImagesAsync(Guid containerId)
    {
        var result = await this.context.Recipes
            .Where(r => r.Id == containerId)
            .Include(r => r.Images)
            .SelectMany(r => r.Images.Select(i => new { i.Name, i.Id}))
            .ToListAsync();
        return this.Ok(result);
    }

    [HttpDelete("{containerId}/image/{imageId}")]
    [BasicAuth]
    public async Task<IActionResult> DeleteImageAsync(Guid containerId, Guid imageId)
    {
        var recipe = await this.context.Recipes.Where(r => r.Id == containerId).Include(r => r.Images).FirstAsync();
        if (recipe == null)
        {
            return NotFound("recipe");
        }
        var img = recipe.Images.FirstOrDefault(i => i.Id == imageId);
        if (img != null)
        {
            recipe.Images.Remove(img);
            context.Images.Remove(img);
            await this.context.SaveChangesAsync();
            return Ok();
        }
        else
        {
            return NotFound("image");
        }
    }
}
