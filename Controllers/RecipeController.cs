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

            if (!this.ModelState.IsValid)
            {
                return BadRequest(this.ModelState);
            }
            // TODO
            // Sending the whole EF object back and forth puts you in a bad state
            var existingRecipe = await _context.GetRecipeAsync(recipe.Id);
            if (existingRecipe == null)
            {
                // create
            }
            else
            {
                _context.Entry(existingRecipe).CurrentValues.SetValues(recipe);
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
