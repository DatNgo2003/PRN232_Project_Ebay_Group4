using Backend.Models;

namespace Backend.Repositories
{
    public interface IReviewRepository
    {
        Task AddReviewAsync(Review review);
        Task<bool> HasUserReviewedProductAsync(int reviewerId, int productId);
    }
}