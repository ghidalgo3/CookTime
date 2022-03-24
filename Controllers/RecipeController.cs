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

    public RecipeController(ApplicationDbContext context)
    {
        this.context = context;
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

    [HttpPost("{recipeId}/migrate")]
    [BasicAuth]
    public async Task<IActionResult> MigrateRecipe(Guid recipeId)
    {
        var recipe = await context.GetRecipeAsync(recipeId);
        if (recipe == null)
        {
            return NotFound();
        }
        var mpRecipe = new MultiPartRecipe(recipe);
        context.MultiPartRecipes.Add(mpRecipe);
        await context.SaveChangesAsync();

        var cart = await context.GetActiveCartAsync();
        cart.RecipeRequirement = cart.RecipeRequirement.Where(rr => rr.Recipe?.Id != recipe.Id).ToList();

        context.Recipes.Remove(recipe);
        await context.SaveChangesAsync();

        return this.RedirectToPage("/Recipes/Details", new { id = mpRecipe.Id });
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

    // PUT: api/Recipe/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    [BasicAuth]
    public async Task<IActionResult> PutRecipe(
        Guid id,
        [FromBody] Recipe recipe)
    {
        if (id != recipe.Id)
        {
            return BadRequest();
        }

        // TODO
        // Sending the whole EF object back and forth puts you in a bad state
        var existingRecipe = await context.GetRecipeAsync(recipe.Id);
        if (existingRecipe == null)
        {
            // create, shouldn't happen because Create Recipe has a dedicated
            // page
        }
        else
        {
            context.Entry(existingRecipe).CurrentValues.SetValues(recipe);
            await CopyIngredients(this.context, recipe, existingRecipe);
            existingRecipe.Steps = recipe.Steps.Where(s => !string.IsNullOrWhiteSpace(s.Text)).ToList();
            // update
            context.Recipes.Update(existingRecipe);
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RecipeExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return Ok(existingRecipe);
    }

    public static async Task CopyIngredients<TRecipeStep, TIngredientRequirement>(
        ApplicationDbContext context,
        IRecipeComponent<TRecipeStep, TIngredientRequirement> payloadComponent,
        IRecipeComponent<TRecipeStep, TIngredientRequirement> existingComponent)
        where TRecipeStep : IRecipeStep
        where TIngredientRequirement : IIngredientRequirement
    {
        var currentIngredients = existingComponent.Ingredients;
        existingComponent.Ingredients = new List<TIngredientRequirement>();
        foreach (var ingredientRequirement in payloadComponent.Ingredients)
        {
            var matching = currentIngredients
                .FirstOrDefault(ir =>
                    ir.Id == ingredientRequirement.Id);
            if (matching == null)
            {
                var existingIngredient = await context.Ingredients
                    .FirstAsync(ingredient => EF.Functions.Like(ingredientRequirement.Ingredient.Name.Trim(), ingredient.Name.Trim()));
                if (existingIngredient == null)
                {
                    // new ingredient
                    ingredientRequirement.Ingredient.Id = Guid.Empty;
                    ingredientRequirement.Ingredient.Name = ingredientRequirement.Ingredient.Name.Trim();
                    context.Ingredients.Add(ingredientRequirement.Ingredient);
                }
                else
                {
                    ingredientRequirement.Ingredient = existingIngredient;
                }
                // new ingredient requirement
                existingComponent.Ingredients.Add(ingredientRequirement);
            }
            else
            {
                // update of existing ingredient requirement
                matching.Quantity = ingredientRequirement.Quantity;
                matching.Unit = ingredientRequirement.Unit;
                matching.Position = ingredientRequirement.Position;
                var ingredient = await context.Ingredients.FindAsync(ingredientRequirement.Ingredient.Id);
                if (ingredient == null)
                {
                    // entirely new ingredient, client chose ID
                    ingredientRequirement.Id = Guid.NewGuid();
                    ingredientRequirement.Ingredient.Name = ingredientRequirement.Ingredient.Name.Trim();
                    matching.Ingredient = ingredientRequirement.Ingredient;
                }
                else if (!currentIngredients.Any(i => i.Ingredient.Id == ingredient.Id))
                {
                    // reassignment of existing ingredient
                    matching.Ingredient = ingredient;
                }
                // Are you actually changing the ingredient being referenced?
                existingComponent.Ingredients.Add(matching);
            }
        }
    }

    [HttpGet("ingredients")]
    public ActionResult<IEnumerable<Ingredient>> GetIngredients(
        [FromQuery(Name = "name")]
        string query)
    {
        // god shield me from this
        var ingredients = context.Ingredients.Where(i => i.Name.ToUpper().Contains(query.ToUpper())).ToList();
        return this.Ok(ingredients);
    }

    // DELETE: api/Recipe/5
    [HttpDelete("{id}")]
    [BasicAuth]
    public async Task<IActionResult> DeleteRecipe(Guid id)
    {
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
        var cart = await context.GetActiveCartAsync();
        cart.RecipeRequirement = cart.RecipeRequirement.Where(rr => rr.Recipe.Id != id).ToList();
        await context.SaveChangesAsync();

        return NoContent();
    }

    private bool RecipeExists(Guid id) => context.Recipes.Any(e => e.Id == id);

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
            .SelectMany(r => r.Images.Select(i => new {Name = i.Name, Id = i.Id}))
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
