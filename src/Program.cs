
using babe_algorithms.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// var recipes = app.MapGroup("/api/recipes");
// recipes.MapGet("/searchByName", async (string name, int limit, int offset, RecipeRepository repo) =>
// {
//     var results = await repo.SearchByNameAsync(name, limit, offset);
//     return Results.Ok(results);
// });
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    Migrations.RunMigrations(logger, builder.Configuration.GetConnectionString("Postgres")!);
}
app.Run();