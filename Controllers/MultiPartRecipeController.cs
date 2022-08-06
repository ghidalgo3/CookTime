using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using babe_algorithms.Services;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Globalization;
using babe_algorithms.Models.Users;
using Microsoft.AspNetCore.Authorization;

namespace babe_algorithms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MultiPartRecipeController : ControllerBase, IImageController
    {
        private readonly ApplicationDbContext _context;

        public ISessionManager Session { get; }

        public TextInfo TextInfo { get; }

        public MultiPartRecipeController(
            ApplicationDbContext context,
            ISessionManager sessionManager)
        {
            _context = context;
            this.Session = sessionManager;
            this.TextInfo = new CultureInfo("en-US",false).TextInfo;
        }

        // GET: api/MultiPartRecipe
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MultiPartRecipe>>> GetMultiPartRecipes()
        {
            return await _context.MultiPartRecipes.ToListAsync();
        }

        [HttpDelete("{id}/favorite")]
        public async Task<ActionResult> DeleteFromFavorites(Guid id)
        {
            var user = await this.Session.GetSignedInUserAsync(this.User);
            if (user == null)
            {
                return this.Unauthorized("You are not signed in.");
            }

            var existingRecipe = await _context.GetMultiPartRecipeAsync(id);
            if (existingRecipe == null)
            {
                return this.NotFound();
            }
            var modified = await this._context.RemoveRecipeFromCart(user, existingRecipe, Cart.Favorites);
            if (modified)
            {
                await this._context.SaveChangesAsync();
            }

            return this.Ok();
        }

        [HttpPut("{id}/favorite")]
        public async Task<ActionResult> AddRecipeToFavorites(Guid id)
        {
            var user = await this.Session.GetSignedInUserAsync(this.User);
            if (user == null)
            {
                return this.Unauthorized("You are not signed in.");
            }

            var existingRecipe = await _context.GetMultiPartRecipeAsync(id);
            if (existingRecipe == null)
            {
                return this.NotFound();
            }
            var modified = await this._context.AddRecipeToCart(user, existingRecipe, Cart.Favorites);
            if (modified)
            {
                await this._context.SaveChangesAsync();
            }
            return this.Ok();
        }

        [HttpDelete("{id}/review")]
        public async Task<ActionResult> DeleteReview(Guid id)
        {
            var user = await this.Session.GetSignedInUserAsync(this.User);
            if (user == null)
            {
                return this.Unauthorized("You are not signed in.");
            }

            var existingRecipe = await _context.GetMultiPartRecipeAsync(id);
            if (existingRecipe == null)
            {
                return this.NotFound();
            }

            var existingReviews = await this._context.GetReviewsAsync(existingRecipe.Id);
            var existingReview = existingReviews.FirstOrDefault(r => r.Owner.Id == user.Id);
            // var existingReview = await this._context.GetReviewAsync(existingRecipe.Id, user.Id);
            if (existingReview == null)
            {
                return this.NotFound();
            }
            else
            {
                existingRecipe.ReviewCount--;
            }
            if (existingRecipe.ReviewCount > 0 )
            {
                existingRecipe.AverageReviews = existingReviews.Select(r => r.Rating).Sum() / (double)existingRecipe.ReviewCount;
            }
            else
            {
                existingRecipe.AverageReviews = 0;
            }
            this._context.Reviews.Remove(existingReview);
            await this._context.SaveChangesAsync();
            return this.Ok();
        }

        [HttpPut("{id}/review")]
        public async Task<ActionResult> PutReview(
            Guid id,
            [FromBody] ReviewDTO review)
        {
            var user = await this.Session.GetSignedInUserAsync(this.User);
            if (user == null)
            {
                return this.Unauthorized("You are not signed in.");
            }

            var existingRecipe = await _context.GetMultiPartRecipeAsync(id);
            if (existingRecipe == null)
            {
                return this.NotFound();
            }

            // if (existingRecipe.Owner.Id == user.Id)
            // {
            //     return this.BadRequest("You cannot review your own recipes.");
            // }
            var existingReviews = await this._context.GetReviewsAsync(existingRecipe.Id);
            var existingReview = existingReviews.FirstOrDefault(r => r.Owner.Id == user.Id);
            // var existingReview = await this._context.GetReviewAsync(existingRecipe.Id, user.Id);
            if (existingReview == null)
            {
                var newReview = new Review()
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    LastModified = DateTimeOffset.UtcNow,
                    Owner = user,
                    Recipe = existingRecipe,
                    Rating = Math.Max(Math.Min(review.Rating, 5), 1),
                    Text = review.Text,
                };
                existingRecipe.ReviewCount += 1;
                existingReviews.Add(newReview);
                this._context.Reviews.Add(newReview);
            }
            else
            {
                existingReview.Rating = review.Rating;
                existingReview.Text = review.Text;
                existingReview.LastModified = DateTimeOffset.UtcNow;
            }
            existingRecipe.AverageReviews = existingReviews.Select(r => r.Rating).Sum() / (double)existingRecipe.ReviewCount;
            await this._context.SaveChangesAsync();
            return this.Ok();
        }
        
        [HttpGet("{id}/reviews")]
        public async Task<ActionResult> GetReviews(Guid id)
        {
            return this.Ok(await this._context.GetReviewsAsync(id));
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
            SetNutritionFacts(multiPartRecipe, body);
            SetDietDetails(multiPartRecipe, body);
            return this.Ok(body);
        }

        private void SetDietDetails(
            MultiPartRecipe multiPartRecipe,
            RecipeNutritionFacts body)
        {
            body.DietDetails.Add(TodaysTenDetails.GetTodaysTenDietDetail(multiPartRecipe.GetAllIngredients()));
            TestKeto(body);
        }

        private static void TestKeto(RecipeNutritionFacts body)
        {
            // start easy... keto means 5% of calories are from carbs?
            var totalCalories = body.Recipe.Calories;
            var totalCarbs = body.Recipe.Carbohydrates;
            var carbCalories = totalCarbs * 4;
            if (carbCalories / totalCalories < 0.05)
            {
                body.DietDetails.Add(
                    new DietDetail()
                    {
                        Opinion = DietOpinion.Recommended,
                        Name = "Keto"
                    });
            }
        }

        private static void SetNutritionFacts(MultiPartRecipe multiPartRecipe, RecipeNutritionFacts body)
        {
            var ingredientDescriptors = new List<IngredientNutritionDescription>();
            foreach (var component in multiPartRecipe.RecipeComponents)
            {
                var allIngredientRequirements = component.Ingredients;
                // irs.Sort((first, second) => first.Position.CompareTo(second.Position));
                var result = allIngredientRequirements.Select(ir =>
                {
                    var nutritionForIngredient = ir.CalculateNutritionFacts();
                    var ingredientDescriptor = ir.GetPartialIngredientDescription();
                    ingredientDescriptor.CaloriesPerServing = nutritionForIngredient.Calories / multiPartRecipe.ServingsProduced;
                    ingredientDescriptors.Add(ingredientDescriptor);
                    return nutritionForIngredient;
                });
                if (result.Any())
                {
                    body.Components.Add(result.Aggregate((a, b) => a + b));
                }
            }
            body.Recipe = body.Components.Aggregate((a, b) => a + b);
            body.Ingredients = ingredientDescriptors;
        }

        // PUT: api/MultiPartRecipe/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
            var user = await this.Session.GetSignedInUserAsync(this.User);
            if (user == null)
            {
                return this.Unauthorized("You are not signed in.");
            }

            var existingRecipe = await _context.GetMultiPartRecipeAsync(id);
            if (existingRecipe.Owner?.Id != payload.Owner?.Id && !this.Session.IsInRole(user, Role.Administrator))
            {
                return this.Unauthorized("You can only edit your own recipes.");
            }

            _context.Entry(existingRecipe).CurrentValues.SetValues(payload);
            await MergeRecipeRelations(payload, existingRecipe);
            existingRecipe.LastModifiedDate = DateTimeOffset.UtcNow;
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
            await ApplyDefaultCategories(payload, existingRecipe);
        }

        private async Task ApplyDefaultCategories(MultiPartRecipe payload, MultiPartRecipe existingRecipe)
        {
            var applicableCategories = existingRecipe.ApplicableDefaultCategories.ToHashSet();
            var currentCategories = existingRecipe.Categories.Select(cat => cat.Name).ToHashSet();
            if (!applicableCategories.IsSubsetOf(currentCategories))
            {
                foreach (var ac in applicableCategories)
                {
                    var toAdd = await this._context.GetCategoryAsync(ac);
                    if (toAdd != null && !existingRecipe.Categories.Contains(toAdd))
                    {
                        existingRecipe.Categories.Add(toAdd);
                    }
                }
            }
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
                    if (Category.DefaultCategories.Select(c => c.ToUpperInvariant()).Contains(category.Name.Trim().ToUpperInvariant()))
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

        [HttpPost("deduplicate")]
        // POST: api/MultiPartRecipe/deduplicate
        public async Task<IActionResult> Deduplicate()
        {
            var user = await this.Session.GetSignedInUserAsync(this.User);
            if (user == null)
            {
                return this.Unauthorized();
            }

            await Program.DeduplicateIngredients(this._context);
            return this.Ok();
        }

        // DELETE: api/MultiPartRecipe/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMultiPartRecipe(Guid id)
        {
            var user = await this.Session.GetSignedInUserAsync(this.User);
            if (user == null)
            {
                return this.Unauthorized("You must be signed in to delete your recipes.");
            }

            var recipe = await _context.GetMultiPartRecipeWithImagesAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            if (recipe.Owner?.Id != user.Id && !this.Session.IsInRole(user, Role.Administrator))
            {
                return this.Unauthorized("You can only delete recipes you own.");
            }
            var reviews = await _context.GetReviewsAsync(recipe.Id);
            _context.Reviews.RemoveRange(reviews);
            _context.MultiPartRecipes.Remove(recipe);
            foreach (var image in recipe.Images)
            {
                _context.Images.Remove(image);
            }
            var recipeOwner = recipe.Owner?.Id == user.Id ? user : recipe.Owner;
            var cart = await _context.GetGroceryListAsync(recipeOwner);
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

    public class ReviewDTO
    {
        public int Rating { get; set; }

        public string Text { get; set; }
    }
}
