using Backend.Models;

namespace Backend.Repositories
{
    public interface IOrderRepository
    {
        Task<OrderTable> CreateSimpleOrderAsync(
            int buyerId,
            int productId,
            decimal unitPrice,
            decimal shippingFee,
            string paymentMethod,
            string paymentStatus,
            string orderStatus,
            string shippingRegion,
            string trackingNumber,
            DateTime estimatedArrival);
        Task<IEnumerable<OrderItem>> GetPurchaseHistoryAsync(int buyerId);
        Task<IEnumerable<OrderItem>> GetOrderItemsBySellerIdAsync(int sellerId);
    }
}
