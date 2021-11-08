using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using babe_algorithms.Models;
using babe_algorithms.Services;

namespace babe_algorithms
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Cart
        [HttpGet]
        public async Task<ActionResult<Cart>> GetCart()
        {
            return await _context.GetActiveCartAsync();
        }

        // GET: api/Cart/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cart>> GetCart(Guid id)
        {
            var cart = await _context.Carts.FindAsync(id);

            if (cart == null)
            {
                return NotFound();
            }

            return cart;
        }

        // PUT: api/Cart/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCart(Guid id, Cart cartPayload)
        {
            if (id != cartPayload.Id)
            {
                return BadRequest();
            }

            var cart = await this._context.GetActiveCartAsync();
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
        public async Task<ActionResult<Cart>> PostCart(Cart cart)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCart", new { id = cart.Id }, cart);
        }

        // DELETE: api/Cart/clear
        [HttpPost("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var cart = await _context.GetActiveCartAsync();
            if (cart == null)
            {
                return NotFound();
            }
            cart.RecipeRequirement.Clear();
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool CartExists(Guid id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }
    }
}
