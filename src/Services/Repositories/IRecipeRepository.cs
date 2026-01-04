using BabeAlgorithms.Models.Contracts;

namespace BabeAlgorithms.Services.Repositories;

public interface IRecipeRepository
{
    Task<Guid> CreateAsync(RecipeCreateDto recipe);
    Task UpdateAsync(RecipeUpdateDto recipe);
    Task DeleteAsync(Guid recipeId);
    Task<RecipeDetailDto?> GetByIdAsync(Guid recipeId);
    Task<List<RecipeSummaryDto>> SearchByNameAsync(string searchTerm);
    Task<List<RecipeSummaryDto>> SearchByIngredientAsync(Guid ingredientId);
    Task<List<RecipeSummaryDto>> GetAllAsync(int pageSize = 50, int pageNumber = 1);
    Task<List<string>> GetImagesAsync(Guid recipeId);
}
