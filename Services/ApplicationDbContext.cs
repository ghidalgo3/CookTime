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
                        // .ThenInclude(ingredient => ingredient.NutritionData)
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

    public async Task<Cart> GetActiveCartAsync(ApplicationUser user)
    {
        var activeCart = await this.Carts
            .Where(c => c.Owner.Id == user.Id)
            .Where(c => c.Active)
            .Include(c => c.RecipeRequirement)
                .ThenInclude(rr => rr.MultiPartRecipe)
                    .ThenInclude(mpRecipe => mpRecipe.RecipeComponents)
                        .ThenInclude(c => c.Ingredients)
                            .ThenInclude(i => i.Ingredient)
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
                Owner = user,
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