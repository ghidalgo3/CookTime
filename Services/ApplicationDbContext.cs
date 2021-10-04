using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using babe_algorithms.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace babe_algorithms.Services
{
    public class ApplicationDbContext : DbContext
    {
        static ApplicationDbContext()
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Unit>();
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<Unit>();
        }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }

        public async Task<Category> GetCategory(Guid id)
        {
            return await this.Categories
                .FirstAsync(category => category.Id == id);
        }

        public async Task<bool> RecipeHasCategory(Guid categoryId, Guid recipeId)
        {
            var recipe = await this.Recipes
                .Include(recipe => recipe.Categories)
                .SingleAsync(recipe => recipe.Id == recipeId);
            return recipe.Categories.Any(c => c.Id == categoryId);
        }

        public async Task<Recipe> GetRecipeAsync(Guid id)
        {
            return await this.Recipes
                .Include(recipe => recipe.Ingredients)
                    .ThenInclude(ir => ir.Ingredient)
                .Include(recipe => recipe.Categories)
                .SingleAsync(recipe => recipe.Id == id);
        }

        public async Task<Cart> GetActiveCartAsync()
        {
            var activeCart = await this.Carts
                .Where(c => c.Active)
                .Include(c => c.RecipeRequirement)
                    .ThenInclude(rr => rr.Recipe)
                        .ThenInclude(recipe => recipe.Ingredients)
                            .ThenInclude(i => i.Ingredient)
                .FirstOrDefaultAsync();
            if (activeCart == null)
            {
                var cart = new Cart()
                {
                    CreateAt = DateTime.Now,
                    Active = true,
                    RecipeRequirement = new List<RecipeRequirement>(),
                };
                this.Carts.Add(cart);
                return cart;
            }
            else
            {
                return activeCart;
            }
        }
    }
}