using BabeAlgorithms.Models.Contracts;

namespace BabeAlgorithms.Services.Repositories;

public interface IIngredientRepository
{
    Task<int> CreateAsync(IngredientCreateDto ingredient);
    Task<IngredientDto?> GetByIdAsync(int ingredientId);
    Task<List<IngredientDto>> SearchAsync(string searchTerm, int limit = 20);
    Task<List<string>> GetImagesAsync(int ingredientId);
}
