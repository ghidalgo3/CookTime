using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using babe_algorithms.Models;
using babe_algorithms.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace babe_algorithms
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            CreateDbIfNotExists(host);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => 
                {
                    logging.AddAzureWebAppDiagnostics();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void CreateDbIfNotExists(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                    using (var conn = (NpgsqlConnection)context.Database.GetDbConnection())
                    {
                        conn.Open();
                        conn.ReloadTypes();
                    }
                    InitializeDatabase(context);
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred creating the DB.");
                throw;
            }
        }

        private static void InitializeDatabase(ApplicationDbContext context)
        {
            var poppySeeds = new Ingredient() { Name = "Poppy seeds" };
            var rolledOats = new Ingredient() { Name = "Rolled oats" };
            var nonDairyMilk = new Ingredient() { Name = "Non-dairy milk" };
            var lemonJuice = new Ingredient() { Name = "Lemon juice" };
            var agave = new Ingredient() { Name = "Agave" };
            var driedBlueberries = new Ingredient() { Name = "Dried blueberries" };
            var sliveredAlmonds = new Ingredient() { Name = "Slivered almonds" };
            context.Ingredients.AddRange(new Ingredient[]
            {
                poppySeeds,
                rolledOats,
                nonDairyMilk,
                lemonJuice,
                agave,
                driedBlueberries,
                sliveredAlmonds,
            });
            var overnightOatsRecipe = new Recipe()
            {
                Name = "Lemon poppy overnight oats",
                Cooktime = TimeSpan.FromMinutes(5),
                ServingsProduced = 4,
                CaloriesPerServing = 290,
                Ingredients = new List<IngredientRequirement>()
                {
                    new IngredientRequirement()
                    {
                        Ingredient = poppySeeds,
                        Unit = Unit.Tablespoon,
                        Quantity = 1,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = rolledOats,
                        Unit = Unit.Cup,
                        Quantity = 2,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = nonDairyMilk,
                        Unit = Unit.Cup,
                        Quantity = 2,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = lemonJuice,
                        Unit = Unit.Tablespoon,
                        Quantity = 3,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = agave,
                        Unit = Unit.Teaspoon,
                        Quantity = 4,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = driedBlueberries,
                        Unit = Unit.Cup,
                        Quantity = 0.25,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = sliveredAlmonds,
                        Unit = Unit.Cup,
                        Quantity = 0.5,
                    },
                }
            };
            overnightOatsRecipe.Directions = @"
#. In a container with a lid, combine poppy seeds, oats, milk, lemon juice, agave, blueberries, and a pinch of salt.
#. Cover and refrigerate the oats overnight or for at least 8h.
#. When you're ready to serve, top with almonds and any other toppings you desire. ";
            context.Recipes.Add(overnightOatsRecipe);
            context.SaveChanges();
        }
    }

}
