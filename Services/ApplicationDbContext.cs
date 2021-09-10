using System;
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

        public DbSet<Recipe>? Recipes { get; set; }
        public DbSet<Ingredient>? Ingredients { get; set; }

        public DbSet<Category>? Categories { get; set; }

        public async Task<Recipe> GetRecipeAsync(Guid id)
        {
            return await this.Recipes
                .Include(recipe => recipe.Ingredients)
                    .ThenInclude(ir => ir.Ingredient)
                .Include(recipe => recipe.Categories)
                .SingleAsync(recipe => recipe.Id == id);
        }
    }
}