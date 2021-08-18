using System;
using System.ComponentModel.DataAnnotations;
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
    }
}