using babe_algorithms.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using babe_algorithms.Models.Users;
using Microsoft.AspNetCore.Identity.UI.Services;
using GustavoTech.Implementation;
using SixLabors.ImageSharp.Web.DependencyInjection;
using BabeAlgorithms.Services.Repositories;

namespace babe_algorithms;

public class Program
{
    private static NpgsqlDataSource dataSource;

    public static void Main(string[] args)
    {
        // Check for export command
        if (args.Length > 0 && args[0] == "export")
        {
            DatabaseExporter.ExportDatabase(args);
            return;
        }

        var builder = WebApplication.CreateBuilder(args);
        builder.Services
        .AddControllersWithViews(options =>
        {
            // TODO enable this.
            // options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        builder.Services.AddRazorPages();
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = !builder.Environment.IsDevelopment();
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            // Cookie settings
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.LoginPath = "/SignIn";
            options.AccessDeniedPath = "/SignIn";
            options.SlidingExpiration = true;
        });

        builder.Services.AddScoped<ISignInManager, SignInManager>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ISessionManager, SessionManager>();
        builder.Services.AddScoped<IEmailSender, EmailSender>();
        builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);
        builder.Services.Configure<AzureOptions>(builder.Configuration.GetSection("Azure"));
        builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));
        builder.Services.AddTransient<IEmailSender, EmailSender>();

        // Configure NpgsqlDataSource for repository pattern
        var connectionString = builder.Configuration.GetConnectionString("Postgres");
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = System.Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_Postgres")
                ?? System.Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? throw new NullReferenceException("Connection string was not found.");
        }
        var npgsqlDataSource = CreateNpgsqlDataSource(connectionString);
        builder.Services.AddSingleton(npgsqlDataSource);

        // Register repositories
        builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
        builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
        builder.Services.AddScoped<IRecipeListRepository, RecipeListRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(npgsqlDataSource);
            options.EnableSensitiveDataLogging(true);
        });
        builder.Services.AddImageSharp(options =>
        {
        })
            .ClearProviders()
            .AddProvider<PostgresImageProvider>(sp =>
            {
                return new PostgresImageProvider(sp);
            });
        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseImageSharp();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseCookiePolicy();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();
        app.MapFallbackToFile("index.html");

        PreStartActions(app, connectionString);

        app.Run();
    }

    /// <summary>
    /// Creates an Npgsql data source. You must only create one instance of NpgsqlDataSource
    /// for each connection string otherwise EF complains that you're creating too many ServiceProviders.
    /// See here: https://github.com/npgsql/efcore.pg/issues/2720
    /// </summary>
    public static NpgsqlDataSource CreateNpgsqlDataSource(string connectionString)
    {
        if (dataSource == null)
        {
            Console.WriteLine("Creating new NpgsqlDataSource");
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.MapEnum<Unit>();
            dataSource = dataSourceBuilder.Build();
            return dataSource;
        }
        else
        {
            Console.WriteLine("Reusing existing NpgsqlDataSource");
            return dataSource;
        }

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

    private static void RunMigrations(ILogger<Program> logger, string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            logger.LogWarning("Connection string not set, skipping migrations");
            return;
        }

        var scriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");
        if (!Directory.Exists(scriptsPath))
        {
            logger.LogWarning("Scripts directory not found at {path}, skipping migrations", scriptsPath);
            return;
        }

        logger.LogInformation("Running database migrations from {path}", scriptsPath);

        try
        {
            using var dataSource = CreateNpgsqlDataSource(connectionString);
            using var connection = dataSource.OpenConnection();

            // Create migration tracker table if it doesn't exist
            var trackerSql = File.ReadAllText(Path.Combine(scriptsPath, "000_migration_tracker.sql"));
            using (var cmd = new NpgsqlCommand(trackerSql, connection))
            {
                cmd.ExecuteNonQuery();
            }

            // Find and execute all numbered SQL files in order
            var sqlFiles = Directory.GetFiles(scriptsPath, "[0-9]*.sql")
                .Where(f => Path.GetFileName(f) != "000_migration_tracker.sql")
                .OrderBy(f => f)
                .ToList();

            foreach (var sqlFile in sqlFiles)
            {
                var filename = Path.GetFileName(sqlFile);

                // Check if migration already applied
                using var checkCmd = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM cooktime.schema_migrations WHERE script_name = @name", connection);
                checkCmd.Parameters.AddWithValue("name", filename);
                var count = (long)checkCmd.ExecuteScalar()!;

                if (count > 0)
                {
                    logger.LogInformation("✓ Skipping {filename} (already applied)", filename);
                    continue;
                }

                logger.LogInformation("→ Applying {filename}...", filename);

                // Execute the migration
                var sql = File.ReadAllText(sqlFile);
                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Calculate checksum
                var checksum = CalculateMD5(sqlFile);

                // Record the migration
                using var recordCmd = new NpgsqlCommand(
                    "INSERT INTO cooktime.schema_migrations (script_name, checksum) VALUES (@name, @checksum)", connection);
                recordCmd.Parameters.AddWithValue("name", filename);
                recordCmd.Parameters.AddWithValue("checksum", checksum);
                recordCmd.ExecuteNonQuery();

                logger.LogInformation("✓ Applied {filename}", filename);
            }

            logger.LogInformation("All migrations complete!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to run migrations");
            throw;
        }
    }

    private static string CalculateMD5(string filePath)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private static void PreStartActions(IHost host, string connectionString)
    {

        var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        RunMigrations(logger, connectionString);
        ConfigureDatabase(logger, services);
        logger.LogInformation("PID = {pid}", Environment.ProcessId);
    }

    private static void ConfigureDatabase(ILogger<Program> logger, IServiceProvider services)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
                using var conn = (NpgsqlConnection)context.Database.GetDbConnection();
                conn.Open();
                conn.ReloadTypes();
                // InitializeDatabase(context);
            }
            else
            {
                LoadFoodData(context);
                LabelNutrients(context);
                // DeduplicateIngredients(context).Wait();
            }
            CreateRoles(roleManager).Wait();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred creating the DB.");
            throw;
        }
    }

    private static void LabelNutrients(ApplicationDbContext context)
    {
        var allIngredients = context
            .Ingredients
            .Include(i => i.NutritionData)
            .Include(i => i.BrandedNutritionData)
            .ToList();
        foreach (var ingredient in allIngredients)
        {
            if (ingredient.BrandedNutritionData == null)
            {
                ingredient.NutritionData = context.SearchSRNutritionData(ingredient.Name);
            }
        }
        context.SaveChanges();
    }

    public static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = Enum.GetValues(typeof(Role)).Cast<Role>().Select(r => r.ToString()).ToArray();
        foreach (var roleName in roleNames)
        {
            // creating the roles and seeding them to the database
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    public static async Task DeduplicateIngredients(ApplicationDbContext context)
    {
        var allIngredients = await context.Ingredients.ToListAsync();
        var freq = new Dictionary<Ingredient, List<Ingredient>>();
        // The dictionary should be the size of the ingredients table
        // Entry _values_ are the duplicates, but later we will choose to keep
        // one of them
        foreach (var ingredient in allIngredients)
        {
            if (freq.ContainsKey(ingredient))
            {
                freq[ingredient].Add(ingredient);
            }
            else
            {
                freq[ingredient] = new List<Ingredient>();
            }
        }
        var duplicateIngredientFrequencies = freq.Where(kvpPair => kvpPair.Value.Count > 0);
        var duplicateIngredients = duplicateIngredientFrequencies.Select(kvPair => kvPair.Key);
        // fix up recipes so they only reference one of the duplicate ingredients
        foreach (var duplicateIngredient in duplicateIngredients)
        {
            // var toKeep = SelectIngredientToKeep(
            //     duplicateIngredientFrequencies.FirstOrDefault(i => i.Equals(duplicateIngredient)));
            // all these recipes contain a duplicate ingredient
            var recipes = context.GetRecipesWithIngredient(duplicateIngredient.Name).ToList();
            foreach (var recipe in recipes)
            {
                recipe.ReplaceIngredient(
                    i => i.Equals(duplicateIngredient) && i.Id != duplicateIngredient.Id,
                    duplicateIngredient);
            }
        }
        context.SaveChanges();
        // one of the duplicates is "priviledged", the others will be removed.
        foreach (var ingredient in duplicateIngredients)
        {
            var toRemove = allIngredients.Where(ing => ing.Equals(ingredient) && ing.Id != ingredient.Id);
            foreach (var ingredientToRemove in toRemove)
            {
                context.Ingredients.Remove(ingredientToRemove);
            }
        }
        context.SaveChanges();
    }

    private static void LoadFoodData(ApplicationDbContext context)
    {
        LoadSrLegacy(context);
        LoadBrandedFoods(context);
    }

    private static void LoadBrandedFoods(ApplicationDbContext context)
    {
        var fileName = "FoodData_Central_branded_food_json_2021-10-28.json";
        // var fileName = "Small.json";
        int i = 0;
        if (File.Exists(fileName) && !context.BrandedNutritionData.Any())
        {
            foreach (var line in File.ReadLines(fileName))
            {
                try
                {
                    var food = JsonNode.Parse(line.TrimEnd(','));
                    var foodData = new BrandedNutritionData()
                    {
                        GtinUpc = food["gtinUpc"].GetValue<string>(),
                        Ingredients = food["ingredients"].GetValue<string>(),
                        ServingSize = food["servingSize"].GetValue<double>(),
                        ServingSizeUnit = food["servingSizeUnit"].GetValue<string>(),
                        FdcId = food["fdcId"].GetValue<int>(),
                        Description = food["description"].GetValue<string>(),
                        BrandedFoodCategory = food["brandedFoodCategory"].GetValue<string>(),
                        FoodNutrients = JsonDocument.Parse(food["foodNutrients"].ToJsonString()),
                        LabelNutrients = JsonDocument.Parse(food["labelNutrients"].ToJsonString()),
                    };
                    context.BrandedNutritionData.Add(foodData);
                    i++;
                    if (i >= 100)
                    {
                        context.SaveChanges();
                        i = 0;
                    }
                }
                catch
                {
                }
            }
        }
    }

    private static void LoadSrLegacy(ApplicationDbContext context)
    {
        var fileName = "FoodData_Central_sr_legacy_food_json_2021-10-28.json";
        if (File.Exists(fileName) && !context.SRNutritionData.Any())
        {
            var foods = JsonNode.Parse(File.ReadAllText(fileName));
            foreach (var food in foods["SRLegacyFoods"].AsArray())
            {
                var foodData = new StandardReferenceNutritionData()
                {
                    NdbNumber = food["ndbNumber"].GetValue<int>(),
                    FdcId = food["fdcId"].GetValue<int>(),
                    Description = food["description"].GetValue<string>(),
                    FoodNutrients = JsonDocument.Parse(food["foodNutrients"].ToJsonString()),
                    NutrientConversionFactors = JsonDocument.Parse(food["nutrientConversionFactors"].ToJsonString()),
                    FoodCategory = JsonDocument.Parse(food["foodCategory"].ToJsonString()),
                    FoodPortions = JsonDocument.Parse(food["foodPortions"].ToJsonString()),
                };
                context.SRNutritionData.Add(foodData);
            }
            context.SaveChanges();
        }
    }
}
