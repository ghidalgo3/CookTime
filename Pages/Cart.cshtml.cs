using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using babe_algorithms;
using babe_algorithms.Services;

namespace babe_algorithms.Pages
{
    public class CartModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CartModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.Cart ActiveCart { get;set; }

        public async Task OnGetAsync()
        {
            ActiveCart = await _context.GetActiveCartAsync();
        }
    }
}
