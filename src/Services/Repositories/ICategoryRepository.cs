using BabeAlgorithms.Models.Contracts;

namespace BabeAlgorithms.Services.Repositories;

public interface ICategoryRepository
{
    Task<int> CreateAsync(CategoryCreateDto category);
    Task<List<CategoryDto>> GetAllAsync();
}
