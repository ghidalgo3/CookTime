using BabeAlgorithms.Models.Contracts;

namespace BabeAlgorithms.Services.Repositories;

public interface IRecipeListRepository
{
    Task<Guid> CreateAsync(RecipeListCreateDto recipeList);
    Task<List<RecipeListDto>> GetByUserIdAsync(Guid userId);
    Task<RecipeListWithRecipesDto?> GetWithRecipesAsync(Guid listId);
    Task AddRecipeAsync(Guid listId, Guid recipeId, double quantity);
    Task RemoveRecipeAsync(Guid listId, Guid recipeId);
}
