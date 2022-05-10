using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Npgsql;
using babe_algorithms.Models.Users;

namespace babe_algorithms.Services;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresEnum<Unit>();
        modelBuilder.Entity<MultiPartRecipe>()
        .HasGeneratedTsVectorColumn(
            p => p.SearchVector,
            "english",  // Text search config
            p => new { p.Name })  // Included properties
        .HasIndex(p => p.SearchVector)
        .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)
    }

    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<MultiPartRecipe> MultiPartRecipes { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<StandardReferenceNutritionData> SRNutritionData { get; set; }
    public DbSet<BrandedNutritionData> BrandedNutritionData { get; set; }

    public IQueryable<MultiPartRecipe> SearchRecipesByName(string search)
    {
        return this.MultiPartRecipes.Where(r => r.SearchVector.Matches(search));
    }

    public IQueryable<MultiPartRecipe> SearchRecipesByTag(string search)
    {
        return this.MultiPartRecipes.Include(mpr => mpr.Categories).Where(r => r.Categories.Any(c => c.Name.ToUpper().Contains(search.ToUpper())));
    }

    public async Task<Review> GetReviewAsync(Guid recipeId, string userId)
    {
        return await this.Reviews
            .Where(r => r.Owner.Id.Equals(userId) && r.Recipe.Id == recipeId)
            .SingleOrDefaultAsync();
    }

    public async Task<List<Review>> GetReviewsAsync(Guid recipeId)
    {
        return await this.Reviews
            .Include(r => r.Owner)
            .Where(r => r.Recipe.Id == recipeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ingredient>> GetIngredientsForAutosuggest(string name)
    {
        var initialQUery = (await this.Ingredients
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Where(ingredient => EF.Functions.Like(ingredient.Name, name))
                        .ToListAsync());
        var x = initialQUery.SelectMany(ingredient =>
        {
            return ingredient.Name.Split(";").Select(name =>
            {
                return new Ingredient()
                {
                    Id = ingredient.Id,
                    Name = name,
                    ExpectedUnitMass = ingredient.ExpectedUnitMass,
                };
            });
        });
        return x;
    }

    public IQueryable<MultiPartRecipe> GetRecipesWithIngredient(string ingredient)
    {
        return this.MultiPartRecipes
            .Include(mpr => mpr.RecipeComponents)
                .ThenInclude(component => component.Ingredients)
                    .ThenInclude(ingredient => ingredient.Ingredient)
            .Where(mpRecipe =>
                mpRecipe.RecipeComponents.Any(rc =>
                    rc.Ingredients.Any(ir =>
                            ir.Ingredient.Name.Trim().ToUpper().Contains(ingredient.Trim().ToUpper()))));
    }

    public async Task<List<Ingredient>> GetIngredients()
    {
        return await this.Ingredients
            .Include(ingredient => ingredient.NutritionData)
            .Include(ingredient => ingredient.BrandedNutritionData)
            .ToListAsync();
    }

    public async Task<Category> GetCategory(Guid id)
    {
        return await this.Categories
            .FirstOrDefaultAsync(category => category.Id == id);
    }

    public async Task<Category> GetCategoryAsync(string name)
    {
        return await this.Categories
            .Where(c => c.Name.ToUpper().Equals(name.ToUpper()))
            .SingleOrDefaultAsync();
    }

    public async Task<bool> RecipeHasCategory(Guid categoryId, Guid recipeId)
    {
        var recipe = await this.Recipes
            .Include(recipe => recipe.Categories)
            .SingleAsync(recipe => recipe.Id == recipeId);
        return recipe.Categories.Any(c => c.Id == categoryId);
    }

    public async Task<MultiPartRecipe> GetMultiPartRecipeAsync(Guid id)
    {
        return await this.MultiPartRecipes
            .Include(mpr => mpr.Owner)
            .Include(mpr => mpr.RecipeComponents)
                .ThenInclude(component => component.Ingredients)
                    .ThenInclude(ingredient => ingredient.Ingredient)
                        .ThenInclude(ingredient => ingredient.NutritionData)
            .Include(mpr => mpr.RecipeComponents)
                .ThenInclude(component => component.Steps)
            .Include(recipe => recipe.Categories)
            .Include(recipe => recipe.Images)
            .SingleOrDefaultAsync(recipe => recipe.Id == id);
    }

    public async Task<MultiPartRecipe> GetMultiPartRecipeNutritionDataAsync(Guid id)
    {
        return await this.MultiPartRecipes
            .Include(mpr => mpr.RecipeComponents)
                .ThenInclude(component => component.Ingredients)
                    .ThenInclude(ingredient => ingredient.Ingredient)
                        .ThenInclude(ingredient => ingredient.NutritionData)
            .Include(mpr => mpr.RecipeComponents)
                .ThenInclude(component => component.Ingredients)
                    .ThenInclude(ingredient => ingredient.Ingredient)
                        .ThenInclude(ingredient => ingredient.BrandedNutritionData)
            .SingleOrDefaultAsync(recipe => recipe.Id == id);
    }

    public async Task<Recipe> GetRecipeAsync(Guid id)
    {
        return await this.Recipes
            .Include(recipe => recipe.Ingredients)
                .ThenInclude(ir => ir.Ingredient)
            .Include(recipe => recipe.Categories)
            .Include(recipe => recipe.Images)
            .SingleOrDefaultAsync(recipe => recipe.Id == id);
    }

    public Ingredient GetIngredient(Guid id)
    {
        return this.Ingredients
            .Include(i => i.BrandedNutritionData)
            .Include(i => i.NutritionData)
        .Single(i => i.Id == id);
    }

    public async Task<Cart> GetGroceryListAsync(ApplicationUser user)
    {
        return await GetCartAsync(user, Cart.DefaultName);
    }

    public async Task<Cart> GetFavoritesAsync(ApplicationUser user)
    {
        return await GetCartAsync(user, Cart.Favorites, simple: true);
    }

    public async Task<Cart> GetCartAsync(ApplicationUser user, string name, bool simple = false)
    {
        var activeCart = 
            simple ? 
                await GetSimpleActiveCartQuery(user, name).FirstOrDefaultAsync() : 
                await GetActiveCartQuery(user, name).FirstOrDefaultAsync();
        if (activeCart == null)
        {
            var cart = new Cart()
            {
                CreateAt = DateTime.Now,
                Active = true,
                Owner = user,
                Name = name,
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

    public IQueryable<MultiPartRecipe> GetRecipes(ApplicationUser user)
    {
        return this.MultiPartRecipes
            .Where(recipe => recipe.Owner.Id == user.Id)
            .Include(r => r.Images)
            .Include(r => r.Categories)
            .Include(recipe => recipe.Owner);
    }

    public IQueryable<Cart> GetActiveCartQuery(
        ApplicationUser user,
        string name)
    {
        return this.Carts
                            .Where(c => c.Name.Equals(name))
                            .Where(c => c.Active)
                            .Where(c => c.Owner.Id == user.Id)
                            .AsSplitQuery()
                            .Include(c => c.RecipeRequirement)
                                .ThenInclude(rr => rr.MultiPartRecipe)
                                    .ThenInclude(mpRecipe => mpRecipe.RecipeComponents)
                                        .ThenInclude(c => c.Ingredients)
                                            .ThenInclude(i => i.Ingredient)
                            .Include(c => c.RecipeRequirement)
                                .ThenInclude(rr => rr.MultiPartRecipe)
                                    .ThenInclude(mpRecipe => mpRecipe.Images)
                            .Include(c => c.RecipeRequirement)
                                .ThenInclude(rr => rr.MultiPartRecipe)
                                    .ThenInclude(mpRecipe => mpRecipe.Categories);
    }
    
    public IQueryable<Cart> GetSimpleActiveCartQuery(ApplicationUser user, string name)
    {
        return this.Carts
                            .Where(c => c.Name.Equals(name))
                            .Where(c => c.Active)
                            .Where(c => c.Owner.Id == user.Id)
                            .Include(c => c.RecipeRequirement)
                                .ThenInclude(rr => rr.MultiPartRecipe);
    }

    public async Task<bool> AddRecipeToCart(
        ApplicationUser user,
        MultiPartRecipe recipe,
        string cartName)
    {
        var cart = await this.GetCartAsync(user, cartName);
        if (cart.ContainsRecipe(recipe))
        {
            return false;
        }
        else
        {
            cart.RecipeRequirement.Add(new RecipeRequirement()
            {
                MultiPartRecipe = recipe,
                Quantity = 1.0,
            });
            return true;
        }

    }

    public async Task<bool> RemoveRecipeFromCart(ApplicationUser user, MultiPartRecipe recipe, string cartName)
    {
        var cart = await this.GetCartAsync(user, cartName);
        if (cart.ContainsRecipe(recipe))
        {
            var removalIndex = cart.RecipeRequirement.FindIndex(rr => rr.MultiPartRecipe.Id == recipe.Id);
            cart.RecipeRequirement.RemoveAt(removalIndex);
            return true;
        }
        else
        {
            return false;
        }
    }
}