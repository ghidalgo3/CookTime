using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using babe_algorithms;
using babe_algorithms.Services;

namespace babe_algorithms.Pages.Recipes
{
    public class DetailsModel : PageModel
    {
        private readonly babe_algorithms.Services.ApplicationDbContext _context;

        public DetailsModel(babe_algorithms.Services.ApplicationDbContext context)
        {
            _context = context;
        }

        public Recipe Recipe { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Recipe = await _context.Recipes
                .Include(recipe => recipe.Ingredients)
                    .ThenInclude(ir => ir.Ingredient)
                .Include(recipe => recipe.Categories)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Recipe == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
