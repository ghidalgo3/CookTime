using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using babe_algorithms.Services;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Globalization;

namespace babe_algorithms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MultiPartRecipeController : ControllerBase, IImageController
    {
        private readonly ApplicationDbContext _context;

        public TextInfo TextInfo { get; }

        public MultiPartRecipeController(ApplicationDbContext context)
        {
            _context = context;
            this.TextInfo = new CultureInfo("en-US",false).TextInfo;
        }

        // GET: api/MultiPartRecipe
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MultiPartRecipe>>> GetMultiPartRecipes()
        {
            return await _context.MultiPartRecipes.ToListAsync();
        }

        // GET: api/MultiPartRecipe/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MultiPartRecipe>> GetMultiPartRecipe(Guid id)
        {
            var multiPartRecipe = await _context.GetMultiPartRecipeAsync(id);

            if (multiPartRecipe == null)
            {
                return NotFound();
            }

            return multiPartRecipe;
        }

        // GET: api/MultiPartRecipe/5/nutritionData
        [HttpGet("{id}/nutritionData")]
        public async Task<ActionResult<RecipeNutritionFacts>> GetMultiPartRecipeNutritionData(Guid id)
        {
            var multiPartRecipe = await _context.GetMultiPartRecipeNutritionDataAsync(id);
            if (multiPartRecipe == null)
            {
                return NotFound();
            }
            var body = new RecipeNutritionFacts();
            var ingredientDescriptors = new List<IngredientNutritionDescription>();
            foreach (var component in multiPartRecipe.RecipeComponents)
            {
                var allIngredientRequirements = component.Ingredients;
                // irs.Sort((first, second) => first.Position.CompareTo(second.Position));
                var result = allIngredientRequirements.Select(ir => {
                    var nutritionForIngredient = ir.CalculateNutritionFacts();
                    var ingredientDescriptor = ir.GetPartialIngredientDescription();
                    ingredientDescriptor.CaloriesPerServing = nutritionForIngredient.Calories / multiPartRecipe.ServingsProduced;
                    ingredientDescriptors.Add(ingredientDescriptor);
                    return nutritionForIngredient;
                });
                body.Components.Add(result.Aggregate((a,b) => a + b));
            }
            body.Recipe = body.Components.Aggregate((a, b) => a + b);
            body.Ingredients = ingredientDescriptors;
            return this.Ok(body);
        }

        // PUT: api/MultiPartRecipe/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [BasicAuth]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMultiPartRecipe(
            Guid id,
            [FromBody]
            MultiPartRecipe payload)
        {
            if (id != payload.Id)
            {
                return BadRequest();
            }
            var existingRecipe = await _context.GetMultiPartRecipeAsync(id);
            _context.Entry(existingRecipe).CurrentValues.SetValues(payload);
            await MergeRecipeRelations(payload, existingRecipe);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MultiPartRecipeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return this.Ok(existingRecipe);
        }

        private async Task MergeRecipeRelations(MultiPartRecipe payload, MultiPartRecipe existingRecipe)
        {
            await MergeCategories(payload, existingRecipe);
            await MergeComponents(payload, existingRecipe);
        }

        private async Task MergeCategories(MultiPartRecipe payload, MultiPartRecipe existingRecipe)
        {
            var currentCategories = existingRecipe.Categories;
            existingRecipe.Categories = new HashSet<Category>();
            foreach (var category in payload.Categories)
            {
                var existingCategory = currentCategories.FirstOrDefault(c => c.Id == category.Id);
                if (existingCategory != null)
                {
                    // category exists, add it back.
                    existingRecipe.Categories.Add(existingCategory);

                }
                else if (await this._context.GetCategoryAsync(category.Name) is Category existing)
                {
                    // category exists, adding it to this recipe
                    existingRecipe.Categories.Add(existing);
                }
                else
                {
                    var allowedCategories = new List<string>()
                    {
                        "Breakfast",
                        "Lunch",
                        "Dinner",
                        "Snack",
                        "Appetizer",
                        "Side dish",
                        "Main dish",
                        "Dessert",
                        "Drink",
                        "Alcohol",
                        "Brunch",
                        "Plant-based",
                        "Vegetarian",
                        "Salad",
                        "Soup",
                        "Baked goods",
                        "Holiday",
                        "Brazilian",
                        "Indian",
                        "Venezuelan",
                        "Korean",
                        "French",
                        "Sauce",
                        "Seasoning"
                    };

                    if (allowedCategories.Select(c => c.ToUpperInvariant()).Contains(category.Name.Trim().ToUpperInvariant()))
                    {
                        // entirely new category
                        category.Name = this.TextInfo.ToTitleCase(category.Name.Trim());
                        category.Id = Guid.Empty;
                        existingRecipe.Categories.Add(category);
                    }
                }
            }
        }

        private async Task MergeComponents(MultiPartRecipe payload, MultiPartRecipe existingRecipe)
        {
            var currentComponents = existingRecipe.RecipeComponents;
            existingRecipe.RecipeComponents = new List<RecipeComponent>();
            foreach (var component in payload.RecipeComponents)
            {
                var existingComponent = currentComponents.FirstOrDefault(c => c.Id == component.Id);
                if (existingComponent != null)
                {
                    _context.Entry(existingComponent).CurrentValues.SetValues(component);
                    existingComponent.Steps = component.Steps.Where(s => !string.IsNullOrWhiteSpace(s.Text)).ToList();
                    await RecipeController.CopyIngredients(this._context, component, existingComponent);
                    if (!existingComponent.IsEmpty())
                    {
                        existingRecipe.RecipeComponents.Add(existingComponent);
                    }
                }
                else
                {
                    // new component
                    var newComponent = new RecipeComponent()
                    {
                        Name = component.Name,
                        Position = component.Position,
                        Steps = component.Steps,
                    };
                    await RecipeController.CopyIngredients(this._context, component, newComponent);
                    if (!newComponent.IsEmpty())
                    {
                        existingRecipe.RecipeComponents.Add(newComponent);
                    }
                }
            }
        }

        [BasicAuth]
        [HttpPost("deduplicate")]
        // POST: api/MultiPartRecipe/deduplicate
        public async Task<IActionResult> Deduplicate()
        {
            await Program.DeduplicateIngredients(this._context);
            return this.Ok();
        }

        // DELETE: api/MultiPartRecipe/5
        [BasicAuth]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMultiPartRecipe(Guid id)
        {
            var recipe = await _context.GetMultiPartRecipeAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            _context.MultiPartRecipes.Remove(recipe);
            foreach (var image in recipe.Images)
            {
                _context.Images.Remove(image);
            }
            var cart = await _context.GetActiveCartAsync();
            cart.RecipeRequirement = cart.RecipeRequirement.Where(rr => rr.MultiPartRecipe.Id != id).ToList();
            await _context.SaveChangesAsync();

            return NoContent();
        }

    [HttpPut("{containerId}/image")]
    public async Task<IActionResult> PutImageAsync(
        [FromRoute] Guid containerId,
        [FromForm] List<IFormFile> files)
    {
        var recipe = await this._context.MultiPartRecipes.Include(r => r.Images).FirstAsync(r => r.Id == containerId);
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
            _context.Images.Remove(toRemove);
            recipe.Images.Clear();
        }
        recipe.Images.Add(image);
        _context.Images.Add(image);
        await this._context.SaveChangesAsync();
        return this.Ok();
    }

    [HttpGet("{containerId}/images")]
    public async Task<IActionResult> ListImagesAsync(Guid containerId)
    {
        var result = await this._context.MultiPartRecipes
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
        var recipe = await this._context.MultiPartRecipes.Where(r => r.Id == containerId).Include(r => r.Images).FirstAsync();
        if (recipe == null)
        {
            return NotFound("recipe");
        }
        var img = recipe.Images.FirstOrDefault(i => i.Id == imageId);
        if (img != null)
        {
            recipe.Images.Remove(img);
            _context.Images.Remove(img);
            await this._context.SaveChangesAsync();
            return Ok();
        }
        else
        {
            return NotFound("image");
        }
    }

        private bool MultiPartRecipeExists(Guid id)
        {
            return _context.MultiPartRecipes.Any(e => e.Id == id);
        }
    }
}
