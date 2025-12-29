using BabeAlgorithms.Models.Contracts;

namespace BabeAlgorithms.Services.Repositories;

public interface IRecipeListRepository
{
    Task<int> CreateAsync(RecipeListCreateDto recipeList);
    Task<List<RecipeListDto>> GetByUserIdAsync(string userId);
    Task<RecipeListWithRecipesDto?> GetWithRecipesAsync(int listId);
    Task AddRecipeAsync(int listId, int recipeId, int servings);
    Task RemoveRecipeAsync(int listId, int recipeId);
}
