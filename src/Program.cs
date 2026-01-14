
using Azure.Storage.Blobs;
using babe_algorithms.Services;
using babe_algorithms.ViewComponents;
using BabeAlgorithms.Models.Contracts;
using BabeAlgorithms.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")!;
builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddSingleton<CookTimeDB>();

var app = builder.Build();
app.MapGet("/api/category/list", () => Constants.DefaultCategories);
app.MapGet(
    "/api/multipartrecipe",
    async (CookTimeDB cooktime, string? search, int page = 1, int pageSize = 30) =>
{
    List<RecipeSummaryDto> queried_recipes;
    long total_count;

    if (!string.IsNullOrWhiteSpace(search))
    {
        queried_recipes = await cooktime.SearchRecipesAsync(search, pageSize, page);
        total_count = queried_recipes.Count;
    }
    else
    {
        queried_recipes = await cooktime.GetRecipesAsync(pageSize, page);
        total_count = await cooktime.GetRecipeCountAsync();
    }
    return new PagedResult<RecipeSummaryDto>
    {
        Results = queried_recipes,
        CurrentPage = page,
        PageCount = (int)Math.Ceiling((double)total_count / pageSize),
        PageSize = pageSize,
        RowCount = (int)total_count
    };
});
app.MapGet("/api/multipartrecipe/new", async (CookTimeDB cooktime) =>
{
    return await cooktime.GetNewRecipesAsync();
});
app.MapGet("/api/multipartrecipe/featured", async (CookTimeDB cooktime) =>
{
    return await cooktime.GetFeaturedRecipesAsync();
});

// var recipes = app.MapGroup("/api/recipes");
// recipes.MapGet("/searchByName", async (string name, int limit, int offset, RecipeRepository repo) =>
// {
//     var results = await repo.SearchByNameAsync(name, limit, offset);
//     return Results.Ok(results);
// });
using (var scope = app.Services.CreateScope())
{
    var dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    Migrations.RunMigrations(logger, dataSource);

    var blobConnectionString = builder.Configuration.GetConnectionString("AzureBlobStorage");
    var blobServiceClient = new BlobServiceClient(blobConnectionString);
    var blobContainer = blobServiceClient.GetBlobContainerClient("images");

    await Loader.LoadAsync(dataSource, blobContainer, builder.Environment.ContentRootPath);
}
app.Run();