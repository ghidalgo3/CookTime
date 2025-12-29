using BabeAlgorithms.Models.Contracts;

namespace BabeAlgorithms.Services.Repositories;

public interface IReviewRepository
{
    Task<List<ReviewDto>> GetByRecipeIdAsync(int recipeId);
}
