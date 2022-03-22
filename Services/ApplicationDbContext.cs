using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace babe_algorithms.Services;
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

    public IQueryable<MultiPartRecipe> SearchRecipes(string search)
    {
        return this.MultiPartRecipes.Where(r => r.SearchVector.Matches(search));
    }

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

    public async Task<MultiPartRecipe> GetMultiPartRecipeAsync(Guid id)
    {
        return await this.MultiPartRecipes
            .Include(mpr => mpr.RecipeComponents)
                .ThenInclude(component => component.Ingredients)
                    .ThenInclude(ingredient => ingredient.Ingredient)
            .Include(mpr => mpr.RecipeComponents)
                .ThenInclude(component => component.Steps)
            .Include(recipe => recipe.Categories)
            .Include(recipe => recipe.Images)
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
        return this.Ingredients.Single(i => i.Id == id);
    }

    public async Task<Cart> GetActiveCartAsync()
    {
        var activeCart = await this.Carts
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