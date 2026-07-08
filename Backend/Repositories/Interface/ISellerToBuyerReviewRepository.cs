using Backend.Models;

namespace Backend.Repositories.Interface
{
    public interface ISellerToBuyerReviewRepository
    {
        Task<OrderTable?> GetOrderDetailsAsync(int orderId);

        Task<User?> GetUserByIdAsync(int userId);

        Task<bool> HasBeenReviewedAsync(int orderId, int sellerId);

        Task<SellerToBuyerReview> AddReviewAsync(SellerToBuyerReview review);

        Task<IEnumerable<SellerToBuyerReview>> GetReviewsForBuyerAsync(int buyerId);
    }
}
