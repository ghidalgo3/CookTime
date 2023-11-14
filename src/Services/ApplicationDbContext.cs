using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Npgsql;
using babe_algorithms.Models.Users;
using babe_algorithms.Pages;
using GustavoTech.Implementation;

namespace babe_algorithms.Services;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
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
    public DbSet<MultiPartIngredientRequirement> MultiPartIngredientRequirement { get; set; }
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

    public async Task<IEnumerable<Ingredient>> SearchIngredientsAsync(string query)
    {
        var queryMatch = $"%{query}%";
        Fastenshtein.Levenshtein lev = new(query);
        var list = await this.Ingredients
            .Where(ingredient => EF.Functions.ILike(ingredient.Name.Trim(), queryMatch.Trim()))
            .ToListAsync();

        return list.OrderBy(result => result.Name.Split(";").Select(n => lev.DistanceFrom(n)).Min());
    }

    public async Task<IEnumerable<Ingredient>> GetIngredientsForAutosuggest(string name)
    {
        var initialQUery = await this.Ingredients
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Where(ingredient =>
                            ingredient.Name.ToUpper().Contains(name.ToUpper()))
                        .ToListAsync();
        var x = initialQUery.SelectMany(ingredient =>
        {
            return ingredient.Name.Split(";")
                .Where(i => i.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                .Select(name =>
                {
                    return new Ingredient()
                    {
                        Id = ingredient.Id,
                        Name = name.Trim(),
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

    public IQueryable<MultiPartRecipe> GetRecipesWithIngredient(Guid ingredientId)
    {
        return this.MultiPartRecipes
            .Include(mpr => mpr.RecipeComponents)
                .ThenInclude(component => component.Ingredients)
                    .ThenInclude(ingredient => ingredient.Ingredient)
            .Where(mpRecipe =>
                mpRecipe.RecipeComponents.Any(rc =>
                    rc.Ingredients.Any(ir =>
                            ir.Ingredient.Id == ingredientId)));
    }

    public async Task<List<Ingredient>> GetIngredients(string name = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            return await this.Ingredients
                .Where(i => i.Name.ToUpper().Contains(name.ToUpperInvariant()))
                .Include(ingredient => ingredient.NutritionData)
                .Include(ingredient => ingredient.BrandedNutritionData)
                .ToListAsync();
        }
        else
        {
            return await this.Ingredients
                .Include(ingredient => ingredient.NutritionData)
                .Include(ingredient => ingredient.BrandedNutritionData)
                .ToListAsync();
        }
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

    public async Task<List<PartialRecipeView>> PartialRecipeViewQueryAsync(IQueryable<MultiPartRecipe> startingQuery = null)
    {
        var baseQuery = startingQuery ?? this.MultiPartRecipes.AsSplitQuery();
        var intermediate = await baseQuery
            .Include(r => r.Images)
            .Include(r => r.Categories)
            // EFCore dumb dumb can't translate this query in one go ORMs suck
            .Select(r => new {
                r.Name,
                r.Id,
                Images = r.Images.Select(image => new ImageReference(
                    image.Id,
                    image.Name
                )),
                Categories = r.Categories.Select(c => c.Name),
                r.AverageReviews,
                r.ReviewCount,
                r.CreationDate
            })
            .ToListAsync();

        return intermediate.Select(r => new PartialRecipeView(
                r.Name,
                r.Id,
                r.Images,
                r.Categories,
                r.AverageReviews,
                r.ReviewCount,
                r.CreationDate
            ))
            .ToList();
    }

    public async Task<List<RecipeView>> GetFavoriteRecipeViewAsync(ApplicationUser user)
    {
        var favorites = await this.GetFavoritesAsync(user);
        var initialQuery = this.GetSimpleActiveCartQuery(user, Cart.Favorites);
        var allRecipesQuery = await PartialRecipeViewQueryAsync(
            initialQuery.SelectMany(f =>
                f.RecipeRequirement.Select(rr =>
                    rr.MultiPartRecipe)));
        return allRecipesQuery
            .Select(r => RecipeView.From(r, favorites))
            .ToList();
    }

    public async Task<List<RecipeView>> GetFeaturedRecipeViewAsync(Cart favorites, int count = 3)
    {
        var allRecipesQuery = (await PartialRecipeViewQueryAsync())
            .Where(r => r.Images.Any())
            .OrderBy(r => Guid.NewGuid())
            .Take(count);
        return allRecipesQuery
            .Select(r => RecipeView.From(r, favorites))
            .ToList();
    }

    public async Task<Cart> GetFavoritesView(ApplicationUser user)
    {
        return await GetCartAsync(user, Cart.Favorites, simple: true);
    }

    public async Task<List<RecipeView>> GetOwnedRecipesAsync(ApplicationUser user)
    {
        var init = await PartialRecipeViewQueryAsync(this.MultiPartRecipes.Where(r => r.Owner == user));
        var favorites = await this.GetFavoritesAsync(user);
        return init.Select(r => RecipeView.From(r, favorites)).ToList();
    }

    public async Task<List<RecipeView>> GetNewRecipeViewAsync(Cart favorites, int count = 3)
    {
        var allRecipesQuery = (await PartialRecipeViewQueryAsync())
            .Where(recipe => recipe.CreationDate > DateTimeOffset.UtcNow - TimeSpan.FromDays(7))
            // .Where(r => r.Images.Any())
            .Take(count);
        return allRecipesQuery
            .Select(r => RecipeView.From(r, favorites))
            .ToList();
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
            // .Include(recipe => recipe.Images)
            .SingleOrDefaultAsync(recipe => recipe.Id == id);
    }
    
    public async Task<MultiPartRecipe> GetMultiPartRecipeWithImagesAsync(Guid id)
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
                                            .ThenInclude(ingredient => ingredient.NutritionData)
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

    public async Task MergeRecipeRelationsAsync(MultiPartRecipe payload, MultiPartRecipe existingRecipe)
    {
        await MergeCategories(payload, existingRecipe);
        await MergeComponents(payload, existingRecipe);
        await ApplyDefaultCategories(existingRecipe);
    }

    private async Task MergeCategories(MultiPartRecipe payload, MultiPartRecipe existingRecipe)
    {
        var currentCategories = existingRecipe.Categories;
        existingRecipe.Categories = new HashSet<Category>();
        foreach (var category in payload.Categories)
        {
            var existingCategory = currentCategories.FirstOrDefault(c => c.Id == category.Id);
            if (existingCategory != null)
            {
                // category exists, add it back.
                existingRecipe.Categories.Add(existingCategory);

            }
            else if (await this.GetCategoryAsync(category.Name) is Category existing)
            {
                // category exists, adding it to this recipe
                existingRecipe.Categories.Add(existing);
            }
            else
            {
                if (Category.DefaultCategories.Select(c => c.ToUpperInvariant()).Contains(category.Name.Trim().ToUpperInvariant()))
                {
                    // entirely new category
                    category.Name = category.Name.Trim().ToTitleCase();
                    category.Id = Guid.Empty;
                    existingRecipe.Categories.Add(category);
                }
            }
        }
    }

    private async Task ApplyDefaultCategories(MultiPartRecipe existingRecipe)
    {
        var applicableCategories = existingRecipe.ApplicableDefaultCategories.ToHashSet();
        var currentCategories = existingRecipe.Categories.Select(cat => cat.Name).ToHashSet();
        if (!applicableCategories.IsSubsetOf(currentCategories))
        {
            foreach (var ac in applicableCategories)
            {
                var toAdd = await this.GetCategoryAsync(ac);
                if (toAdd != null && !existingRecipe.Categories.Contains(toAdd))
                {
                    existingRecipe.Categories.Add(toAdd);
                }
            }
        }
    }

    private async Task MergeComponents(MultiPartRecipe payload, MultiPartRecipe existingRecipe)
    {
        var currentComponents = existingRecipe.RecipeComponents;
        existingRecipe.RecipeComponents = new List<RecipeComponent>();
        foreach (var component in payload.RecipeComponents)
        {
            var existingComponent = currentComponents.FirstOrDefault(c => c.Id == component.Id);
            if (existingComponent != null)
            {
                this.Entry(existingComponent).CurrentValues.SetValues(component);
                existingComponent.Steps = component.Steps.Where(s => !string.IsNullOrWhiteSpace(s.Text)).ToList();
                await this.CopyIngredients(component, existingComponent);
                if (!existingComponent.IsEmpty())
                {
                    existingRecipe.RecipeComponents.Add(existingComponent);
                }
            }
            else
            {
                // new component
                var newComponent = new RecipeComponent()
                {
                    Name = component.Name,
                    Position = component.Position,
                    Steps = component.Steps,
                };
                await this.CopyIngredients(component, newComponent);
                if (!newComponent.IsEmpty())
                {
                    existingRecipe.RecipeComponents.Add(newComponent);
                }
            }
        }
    }

    /// <summary>
    /// A recipe can be created spontaneously that uses ingredients which are already in the database.
    /// This method's responsibility is to replace in Ingredient objects with the correct database objects
    /// so that when this recipe is saved to the database it references the existing ingredients.
    /// </summary>
    public async Task LinkImportedRecipeIngredientsAsync(MultiPartRecipe recipe)
    {
        await MergeRecipeRelationsAsync(payload: recipe, existingRecipe: new MultiPartRecipe());
    }

    public async Task CopyIngredients<TRecipeStep, TIngredientRequirement>(
        IRecipeComponent<TRecipeStep, TIngredientRequirement> payloadComponent,
        IRecipeComponent<TRecipeStep, TIngredientRequirement> existingComponent)
        where TRecipeStep : IRecipeStep
        where TIngredientRequirement : IIngredientRequirement
    {
        var currentIngredients = existingComponent.Ingredients;
        existingComponent.Ingredients = new List<TIngredientRequirement>();
        foreach (var ingredientRequirement in payloadComponent.Ingredients)
        {
            var matching = currentIngredients
                .FirstOrDefault(ir =>
                    ir.Id == ingredientRequirement.Id);
            // is this an existing ingredient requirement?
            if (matching == null)
            {
                var existingIngredient = await this.Ingredients.FindAsync(ingredientRequirement.Ingredient.Id)
                    ?? (await this.SearchIngredientsAsync(ingredientRequirement.Ingredient.Name)).FirstOrDefault();
                if (existingIngredient == null)
                {
                    // new ingredient
                    ingredientRequirement.Ingredient.Id = Guid.Empty;
                    ingredientRequirement.Ingredient.Name = ingredientRequirement.Ingredient.Name.Trim();
                    this.Ingredients.Add(ingredientRequirement.Ingredient);
                }
                else
                {
                    // existing ingredient
                    ingredientRequirement.Ingredient = existingIngredient;
                }
                // new ingredient requirement
                existingComponent.Ingredients.Add(ingredientRequirement);
                this.MultiPartIngredientRequirement.Add(ingredientRequirement as MultiPartIngredientRequirement);
            }
            else
            {
                // update of existing ingredient requirement
                matching.Quantity = ingredientRequirement.Quantity;
                matching.Unit = ingredientRequirement.Unit;
                matching.Position = ingredientRequirement.Position;
                matching.Text = ingredientRequirement.Text;
                // (matching as MultiPartIngredientRequirement).RecipeComponentId = (ingredientRequirement as MultiPartIngredientRequirement).RecipeComponentId;
                var ingredient = await this.Ingredients.FindAsync(ingredientRequirement.Ingredient.Id);
                if (ingredient == null)
                {
                    // entirely new ingredient, client chose ID
                    ingredientRequirement.Id = Guid.NewGuid();
                    ingredientRequirement.Ingredient.Name = ingredientRequirement.Ingredient.Name.Trim();
                    matching.Ingredient = ingredientRequirement.Ingredient;
                    this.MultiPartIngredientRequirement.Add(ingredientRequirement as MultiPartIngredientRequirement);
                }
                else if (!currentIngredients.Any(i => i.Ingredient.Id == ingredient.Id))
                {
                    // reassignment of existing ingredient
                    matching.Ingredient = ingredient;
                }
                this.MultiPartIngredientRequirement.Update(matching as MultiPartIngredientRequirement);
                // Are you actually changing the ingredient being referenced?
                existingComponent.Ingredients.Add(matching);
            }
        }
    }
}