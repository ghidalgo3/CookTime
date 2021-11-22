using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using babe_algorithms;
using babe_algorithms.Services;
using babe_algorithms.Models;

namespace babe_algorithms.Pages.Recipes;
public class IndexModel : PageModel
{
    private readonly babe_algorithms.Services.ApplicationDbContext _context;

    public IndexModel(babe_algorithms.Services.ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Recipe> Recipes { get; set; }

    public async Task OnGetAsync()
    {
        Recipes = await _context.Recipes.Include(r => r.Images).ToListAsync();
    }

    public async Task<ActionResult> OnPostAddToCart(Guid recipeId)
    {
        var recipe = await _context.GetRecipeAsync(recipeId);
        if (recipe == null)
        {
            return this.Page();
        }

        var cart = await _context.GetActiveCartAsync();
        var existingRecipe = cart.RecipeRequirement.FirstOrDefault(rr => rr.Recipe.Id == recipeId);
        if (existingRecipe == null)
        {
            cart.RecipeRequirement.Add(new RecipeRequirement()
            {
                Recipe = recipe,
                Quantity = 1.0
            });
        }
        else
        {
            existingRecipe.Quantity += 1.0;
        }
        await _context.SaveChangesAsync();
        return this.RedirectToPage("/Cart");
    }
}
