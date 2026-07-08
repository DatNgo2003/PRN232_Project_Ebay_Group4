using Backend.Models;

namespace Backend.Repositories
{
    public interface IOrderRepository
    {
        Task CreateSimpleOrderAsync(int buyerId, int productId, decimal unitPrice);
        Task<IEnumerable<OrderItem>> GetPurchaseHistoryAsync(int buyerId);
        Task<IEnumerable<OrderItem>> GetOrderItemsBySellerIdAsync(int sellerId);
    }
}