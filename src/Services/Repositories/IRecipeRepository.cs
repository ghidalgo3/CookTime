using BabeAlgorithms.Models.Contracts;

namespace BabeAlgorithms.Services.Repositories;

public interface IRecipeRepository
{
    Task<int> CreateAsync(RecipeCreateDto recipe);
    Task UpdateAsync(RecipeUpdateDto recipe);
    Task DeleteAsync(int recipeId);
    Task<RecipeDetailDto?> GetByIdAsync(int recipeId);
    Task<List<RecipeSummaryDto>> SearchByNameAsync(string searchTerm, int limit = 20, int offset = 0);
    Task<List<RecipeSummaryDto>> SearchByIngredientAsync(string ingredientName, int limit = 20, int offset = 0);
    Task<List<RecipeSummaryDto>> GetAllAsync(int limit = 20, int offset = 0);
    Task<List<string>> GetImagesAsync(int recipeId);
}
