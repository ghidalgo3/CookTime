using babe_algorithms.Services;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using babe_algorithms.Models.Users;

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
                DeduplicateIngredients(context).Wait();
            }
            CreateRoles(roleManager).Wait();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred creating the DB.");
            throw;
        }
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
        var freq = new Dictionary<Ingredient, int>();
        foreach (var ingredient in allIngredients)
        {
            if (freq.ContainsKey(ingredient))
            {
                freq[ingredient]++;
            }
            else
            {
                freq[ingredient] = 1;
            }
        }
        var duplicateIngredientFrequencies = freq.Where(kvpPair => kvpPair.Value > 1);
        var duplicateIngredients = duplicateIngredientFrequencies.Select(kvPair => kvPair.Key);
        // fix up recipes so they only reference one of the duplicate ingredients
        foreach (var duplicateIngredient in duplicateIngredients)
        {
            // all these recipes contain a duplicate ingredient
            var recipes = context.GetRecipesWithIngredient(duplicateIngredient.Name).ToList();
            foreach (var recipe in recipes)
            {
                foreach (var component in recipe.RecipeComponents)
                {
                    foreach (var ingredientRequirement in component.Ingredients)
                    {
                        if (ingredientRequirement.Ingredient.Equals(duplicateIngredient)
                            && ingredientRequirement.Ingredient.Id != duplicateIngredient.Id)
                        {
                            ingredientRequirement.Ingredient = duplicateIngredient;
                        }
                    }
                }
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
                    context.SaveChanges();
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

    private static void InitializeDatabase(ApplicationDbContext context)
    {
        var poppySeeds = new Ingredient() { Name = "Poppy seeds" };
        var rolledOats = new Ingredient() { Name = "Rolled oats" };
        var nonDairyMilk = new Ingredient() { Name = "Non-dairy milk" };
        var lemonJuice = new Ingredient() { Name = "Lemon juice" };
        var agave = new Ingredient() { Name = "Agave" };
        var driedBlueberries = new Ingredient() { Name = "Dried blueberries" };
        var sliveredAlmonds = new Ingredient() { Name = "Slivered almonds" };
        var russetPotatoes = new Ingredient() { Name = "Russet potatoes" };
        var brocolliFlorets = new Ingredient() { Name = "Brocolli florets" };
        var bread = new Ingredient() { Name = "Bread" };
        var garlicCloves = new Ingredient() { Name = "Garlic cloves" };
        var butter = new Ingredient() { Name = "Butter" };
        var whiteMisoPaste = new Ingredient() { Name = "White miso paste" };
        var yellowOnion = new Ingredient() { Name = "Yellow onion" };
        var nutritionalYeast = new Ingredient() { Name = "Nutritional yeast" };
        var vegetableBrothConcentrate = new Ingredient() { Name = "Vegetable broth concentrate" };
        var pieCrust = new Ingredient() { Name = "Pie crust" };
        var tofu = new Ingredient() { Name = "Tofu" };
        var onionCeleyCarrotMix = new Ingredient() { Name = "Onion, celery, carrot mix" };
        var peas = new Ingredient() { Name = "Peas" };
        var allPurposeFlour = new Ingredient() { Name = "All purpose flour" };
        var soySauce = new Ingredient() { Name = "Soy sauce" };
        var vegetableBroth = new Ingredient() { Name = "Vegetable broth" };
        var sage = new Ingredient() { Name = "Sage" };
        var thyme = new Ingredient() { Name = "Thyme" };

        context.Ingredients.AddRange(new Ingredient[]
        {
                poppySeeds,
                rolledOats,
                nonDairyMilk,
                lemonJuice,
                agave,
                driedBlueberries,
                sliveredAlmonds,
                russetPotatoes,
                brocolliFlorets,
                bread,
                garlicCloves,
                butter,
                whiteMisoPaste,
                yellowOnion,
                nutritionalYeast,
                vegetableBrothConcentrate,
                pieCrust,
                tofu,
                onionCeleyCarrotMix,
                peas,
                allPurposeFlour,
                soySauce,
                vegetableBroth,
                sage,
                thyme,
        });

        var breakfastCategory =
            new Category()
            {
                Name = "Breakfast"
            };
        var entreeCategory =
            new Category()
            {
                Name = "Entree"
            };

        context.Categories.AddRange(
            breakfastCategory,
            entreeCategory
        );

        var overnightOatsRecipe = new Recipe()
        {
            Name = "Lemon Poppy Overnight Oats",
            Cooktime = TimeSpan.FromMinutes(5),
            ServingsProduced = 4,
            CaloriesPerServing = 290,
            Categories = new SortedSet<Category>()
                {
                    breakfastCategory,
                },
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
        overnightOatsRecipe.Steps = new List<RecipeStep>()
            {
                new RecipeStep()
                {
                    Text = @"In a container with a lid, combine poppy seeds, oats, milk, lemon juice, agave, blueberries, and a pinch of salt."
                },
                new RecipeStep()
                {
                    Text = @"Cover and refrigerate the oats overnight or for at least 8h."
                },
                new RecipeStep()
                {
                    Text = @"When you're ready to serve, top with almonds and any other toppings you desire."
                }
            };
        context.Recipes.Add(overnightOatsRecipe);

        var broccoliCheddarSoup = new Recipe()
        {
            Name = "Broccoli Cheddar Soup",
            Cooktime = TimeSpan.FromMinutes(40),
            ServingsProduced = 4,
            CaloriesPerServing = 490.0,
            Categories = new HashSet<Category>()
                {
                    entreeCategory,
                },
            Ingredients = new List<IngredientRequirement>()
                {
                    new IngredientRequirement()
                    {
                        Ingredient = russetPotatoes,
                        Quantity = 2,
                        Unit = Unit.Count,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = brocolliFlorets,
                        Quantity = 20,
                        Unit = Unit.Ounce,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = bread,
                        Quantity = 2,
                        Unit = Unit.Count,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = garlicCloves,
                        Quantity = 6,
                        Unit = Unit.Count,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = butter,
                        Quantity = 6,
                        Unit = Unit.Tablespoon,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = whiteMisoPaste,
                        Quantity = 2,
                        Unit = Unit.Tablespoon,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = yellowOnion,
                        Quantity = 2,
                        Unit = Unit.Count,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = nutritionalYeast,
                        Quantity = 4,
                        Unit = Unit.Tablespoon,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = vegetableBrothConcentrate,
                        Quantity = 4,
                        Unit = Unit.Teaspoon,
                    },
                }
        };
        broccoliCheddarSoup.Steps = new List<RecipeStep>()
            {
                new RecipeStep()
                {
                    Text = "Preheat over to 425F. Peel and dice the potato. Roughly chop the broccoli florets into bite-size pieces. Add just 1 cup diced potato to a large pot, cover with 1 inch of water, and add a pinch of salt. Bring to a boil and add just 4 cups chopped broccoli florets. Cover and cook until the potatoes are fork-tender, about 10 to 12 minutes. Drain and transfer vegetables to a blender.",
                },
                new RecipeStep()
                {
                    Text = "Add remaining diced potato and remaining chopped broccoli florets to a baking sheet and toss with 2 Tbsp olive oil and a pinch of salt and pepper. Roast until tender, about 15 to 17 minutes."
                },
                new RecipeStep()
                {
                    Text = "Cut the bread roll into 1/2 inch cubes. Peel and mince the garlic. Add the butter, white miso paste, and just half the minced garlic to a bowl and mix well. Add the cubed bread to a baking sheet and bake until golden brown, about 10 to 12 minutes."
                },
                new RecipeStep()
                {
                    Text = "Peel and dice the onion. Place a medium skillet over medium-high heat with 2 Tbsp olive oil. Once the oil is hot, add the diced onion and remaining minced garlic. Cook until softened, about 3 to 5 minutes, and transfer to the blender with the boiled potatoes and broccoli."
                },
                new RecipeStep()
                {
                    Text = "Add the nutritional yeast, vegetable broth concentrate, 2 cups water, 2 tsp salt, and a pinch of pepper to the blender. Blend the broccoli cheddar soup until smooth, making sure to vent the blender for steam to escape. If necessary, blend the soup in batches, don't overfill the blender."
                },
                new RecipeStep()
                {
                    Text = "Divide the soup between bowls, top with roasted potatoes, roasted broccoli, and miso garlic croutons."
                },
            };
        context.Recipes.Add(broccoliCheddarSoup);

        var vegetablePotPie = new Recipe()
        {
            Name = "Vegetable Pot Pie",
            Cooktime = TimeSpan.FromMinutes(60),
            ServingsProduced = 8,
            Categories = new HashSet<Category>() { entreeCategory },
            Ingredients = new List<IngredientRequirement>()
                {
                    new IngredientRequirement()
                    {
                        Ingredient = pieCrust,
                        Quantity = 2,
                        Unit = Unit.Count,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = tofu,
                        Quantity = 10,
                        Unit = Unit.Ounce,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = onionCeleyCarrotMix,
                        Quantity = 20,
                        Unit = Unit.Ounce,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = peas,
                        Quantity = 0.5,
                        Unit = Unit.Cup,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = garlicCloves,
                        Quantity = 2,
                        Unit = Unit.Count,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = russetPotatoes,
                        Quantity = 1,
                        Unit = Unit.Count,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = allPurposeFlour,
                        Quantity = 0.25,
                        Unit = Unit.Cup,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = nutritionalYeast,
                        Quantity = 2,
                        Unit = Unit.Teaspoon,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = soySauce,
                        Quantity = 2,
                        Unit = Unit.Tablespoon,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = vegetableBroth,
                        Quantity = 1.25,
                        Unit = Unit.Cup,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = sage,
                        Quantity = 1,
                        Unit = Unit.Teaspoon,
                    },
                    new IngredientRequirement()
                    {
                        Ingredient = thyme,
                        Quantity = 1,
                        Unit = Unit.Teaspoon,
                    },
                }
        };
        vegetablePotPie.Steps = new List<RecipeStep>()
            {
                new RecipeStep()
                {
                    Text = "Preheat oven to 400° F.",
                },
                new RecipeStep()
                {
                    Text = "Cut tofu into 1/3-inch dice and press between clean kitchen towels or paper towels to rid of excess water.",
                },
                new RecipeStep()
                {
                    Text = "Heat 1 tablespoon of olive oil in a large skillet over medium heat and cook tofu until golden on all, or most, sides. Remove tofu from skillet and set aside.",
                },
                new RecipeStep()
                {
                    Text = "Heat remaining 2 tablespoons of olive oil in the same skillet. Add onion, celery, carrot, and garlic and sauté until onion is translucent.",
                },
                new RecipeStep()
                {
                    Text = "Add potato to the skillet and cook, stirring frequently, until tender but not mushy.",
                },
                new RecipeStep()
                {
                    Text = "Add flour, nutritional yeast, and soy sauce to the skillet and stir into the vegetables.",
                },
                new RecipeStep()
                {
                    Text = "Add vegetable broth and stir until combined, scraping all the browned bits from the bottom of the pan.",
                },
                new RecipeStep()
                {
                    Text = "Add tofu, peas, sage, and thyme and stir until combined.",
                },
                new RecipeStep()
                {
                    Text = "Remove from heat and season to taste with salt and pepper.",
                },
                new RecipeStep()
                {
                    Text = "Prepare pie crust according to packaging instructions. Fill pie with tofu and vegetable mixture. Place dough over the filling to complete pot pie, press to seal around the edges of the dishes and crimp with a fork. Cut a slit in the middle.",
                },
                new RecipeStep()
                {
                    Text = "Bake in the oven until golden, about 30 minutes.",
                },
            };
        context.Recipes.Add(vegetablePotPie);
        context.SaveChanges();
    }
}
