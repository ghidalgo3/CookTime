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
    public class IndexModel : PageModel
    {
        private readonly babe_algorithms.Services.ApplicationDbContext _context;

        public IndexModel(babe_algorithms.Services.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Recipe> Recipe { get;set; }

        public async Task OnGetAsync()
        {
            Recipe = await _context.Recipes.ToListAsync();
        }
    }
}
