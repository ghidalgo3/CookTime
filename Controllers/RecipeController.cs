using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using babe_algorithms.Services;
using babe_algorithms.Models;

namespace babe_algorithms
{
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

        [HttpGet("units")]
        public ActionResult<IEnumerable<string>> GetUnits()
        {
            return this.Ok(Enum.GetValues<Unit>().Select(v => v.ToString()));
        }

        // GET: api/Recipe/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Recipe>> GetRecipe(Guid id)
        {
            var recipe = await _context.GetRecipeAsync(id);

            if (recipe == null)
            {
                return NotFound();
            }

            return Ok(recipe);
        }

        // PUT: api/Recipe/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
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
            var existingRecipe = await _context.GetRecipeAsync(recipe.Id);
            if (existingRecipe == null)
            {
                // create, shouldn't happen because Create Recipe has a dedicated
                // page
            }
            else
            {
                _context.Entry(existingRecipe).CurrentValues.SetValues(recipe);
                var currentIngredients = existingRecipe.Ingredients;
                existingRecipe.Ingredients = new List<IngredientRequirement>();
                await CopyIngredients(recipe, existingRecipe, currentIngredients);
                existingRecipe.Steps = recipe.Steps.Where(s => !string.IsNullOrWhiteSpace(s.Text)).ToList();
                // update
                _context.Recipes.Update(existingRecipe);
            }
            // var existingRecipe = await _context.GetRecipeAsync(id);
            // if (existingRecipe == null)
            // {
            //     _context.Recipes.Add(recipe);
            //     await _context.SaveChangesAsync();

            //     return CreatedAtAction("GetRecipe", new { id = recipe.Id }, recipe);
            // }
            // _context.Entry(existingRecipe).State = EntityState.Detached;
            // _context.Update(recipe);

            // _context.Entry(recipe).State = EntityState.Modified;
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

        private async Task CopyIngredients(Recipe recipe, Recipe existingRecipe, List<IngredientRequirement> currentIngredients)
        {
            foreach (var ingredientRequirement in recipe.Ingredients)
            {
                var matching = currentIngredients
                    .FirstOrDefault(ir =>
                        ir.Id == ingredientRequirement.Id);
                if (matching == null)
                {
                    var existingIngredient = await _context.Ingredients
                        .FindAsync(ingredientRequirement.Ingredient.Id);
                    if (existingIngredient == null)
                    {
                        ingredientRequirement.Ingredient.Id = Guid.Empty;
                    }
                    // new ingredient requirement
                    existingRecipe.Ingredients.Add(ingredientRequirement);
                }
                else
                {
                    // update of existing ingredient requirement
                    matching.Quantity = ingredientRequirement.Quantity;
                    matching.Unit = ingredientRequirement.Unit;
                    var ingredient = await _context.Ingredients.FindAsync(ingredientRequirement.Ingredient.Id);
                    if (ingredient == null)
                    {
                        // entirely new ingredient, client chose ID
                        ingredientRequirement.Id = Guid.NewGuid();
                        matching.Ingredient = ingredientRequirement.Ingredient;
                    }
                    else if (!currentIngredients.Any(i => i.Ingredient.Id == ingredient.Id))
                    {
                        // reassignment of existing ingredient
                        matching.Ingredient = ingredient;
                    }
                    // Are you actually changing the ingredient being referenced?
                    existingRecipe.Ingredients.Add(matching);
                }
            }
        }

        [HttpGet("ingredients")]
        public ActionResult<IEnumerable<Ingredient>> GetIngredients(
            [FromQuery(Name = "name")]
            string query)
        {
            // god shield me from this
            var ingredients = _context.Ingredients.Where(i => i.Name.ToUpper().Contains(query.ToUpper())).ToList();
            return this.Ok(ingredients);
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
