using babe_algorithms.Services;
using Microsoft.EntityFrameworkCore;
using babe_algorithms.Models;

namespace babe_algorithms.Tests;

public class Mocks
{
    public static ApplicationDbContext GetApplicationDbContext()
    {
        // var connection = new SqliteConnection("Filename=:memory:");
        // connection.Open();
        var dataSource = Program.CreateNpgsqlDataSource("Server=localhost;Database=KookTime;Include Error Detail=true");
        // These options will be used by the context instances in this test suite, including the connection opened above.
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(dataSource)
            .Options;

        // Create the schema and seed some data
        var context = new ApplicationDbContext(contextOptions);
        return context;
    }

    public static void SeedDatabaseIngredients(ApplicationDbContext appDbContext)
    {
        appDbContext.Ingredients.AddRange(new List<Ingredient>()
        {
            new Ingredient()
            {
                Name = "Olive oil"
            },
            new Ingredient()
            {
                Name = "Garlic clove"
            },
            new Ingredient()
            {
                Name = "Whole milk"
            }
        });
        appDbContext.SaveChanges();
    }
}