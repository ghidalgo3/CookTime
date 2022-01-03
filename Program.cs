using babe_algorithms.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace babe_algorithms;

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
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
                using var conn = (NpgsqlConnection)context.Database.GetDbConnection();
                conn.Open();
                conn.ReloadTypes();
            }
            // reinit
            context = services.GetRequiredService<ApplicationDbContext>();
            InitializeDatabase(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred creating the DB.");
            throw;
        }
    }

    /// <summary>
    /// This method must idempotently modify the database to initialize
    /// things like:
    /// 1. Default data
    /// 2. Authorization roles
    /// </summary>
    private static void InitializeDatabase(ApplicationDbContext context)
    {
        SetDefaultTags(context);
    }

    private static void SetDefaultTags(ApplicationDbContext context)
    {
        var defaultTags = new List<Tag>()
        {
            new Tag()
            {
                Name = "Breakfast",
            },
            new Tag()
            {
                Name = "Vegan",
            },
            new Tag()
            {
                Name = "Drink",
            },
            new Tag()
            {
                Name = "Dessert",
            },
        };
        var allKnownTags = context.Tags.ToList();
        bool addedTag = false;
        foreach (var defaultTag in defaultTags)
        {
            if (!allKnownTags.Any(t => t.Name.Equals(defaultTag.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                addedTag = true;
                context.Tags.Add(defaultTag);
            }
        }
        if (addedTag)
        {
            context.SaveChanges();
        }
    }
}
