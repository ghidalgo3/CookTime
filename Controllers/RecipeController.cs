using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using babe_algorithms.Services;

namespace babe_algorithms
{
    public class RecipeViewModel
    {
        public RecipeViewModel()
        {
        }

        public RecipeViewModel(Recipe recipe)
        {
            this.ID = recipe.Id;
            this.Name = recipe.Name;
            this.Duration = recipe.Cooktime.TotalMinutes;
            this.CaloriesPerServing = recipe.CaloriesPerServing;
            this.Servings = recipe.ServingsProduced;
            this.Ingredients = recipe.Ingredients.Select(ingredient => 
            {
                return new IngredientRequirementViewModel()
                {
                    Ingredient = ingredient.Ingredient.Name,
                    Unit = ingredient.Unit.ToString(),
                    Quantity = ingredient.Quantity,
                };
            }).ToArray();
            this.Steps = recipe.Steps.Select(s =>
            {
                return new RecipeStepViewModel()
                {
                    Text = s.Text,
                };
            }).ToArray();
            this.Categories = recipe.Categories.Select(c => c.Name).ToArray();
        }

        public Guid ID { get; set; }
        public string Name { get; set; }
        public double Duration { get; set; }
        public double CaloriesPerServing { get; set; }
        public double Servings { get; set; } 
        public IngredientRequirementViewModel[] Ingredients { get; set; }
        public RecipeStepViewModel[] Steps { get; set; }
        public string[] Categories { get; set; }
    }

    public class IngredientRequirementViewModel
    {
        public string Ingredient { get; set; }
        public string Unit { get; set; }
        public double Quantity { get; set; }
    }

    public class RecipeStepViewModel
    {
        public string Text { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RecipeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Recipe
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipes()
        {
            return await _context.Recipes.ToListAsync();
        }

        // GET: api/Recipe/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeViewModel>> GetRecipe(Guid id)
        {
            var recipe = await _context.GetRecipeAsync(id);

            if (recipe == null)
            {
                return NotFound();
            }

            return new RecipeViewModel(recipe);
        }

        // PUT: api/Recipe/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecipe(
            Guid id,
            Recipe recipe)
        {
            if (id != recipe.Id)
            {
                return BadRequest();
            }

            _context.Entry(recipe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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

            return NoContent();
        }

        // POST: api/Recipe
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Recipe>> PostRecipe(Recipe recipe)
        {
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRecipe", new { id = recipe.Id }, recipe);
        }

        // DELETE: api/Recipe/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(Guid id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RecipeExists(Guid id)
        {
            return _context.Recipes.Any(e => e.Id == id);
        }
    }
}
