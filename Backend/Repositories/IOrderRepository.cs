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
            int addressId,              
            string trackingNumber,
            DateTime estimatedArrival,
            int quantity,
            decimal subTotal,
            decimal discountAmount,
            decimal totalAmount,
            int? couponId);

        Task<IEnumerable<OrderItem>> GetPurchaseHistoryAsync(int buyerId);
        Task<IEnumerable<OrderItem>> GetOrderItemsBySellerIdAsync(int sellerId);
        Task<OrderTable?> GetOrderWithDetailsAsync(int orderId);
        Task UpdateShippingStatusAsync(int orderId, string newShippingStatus);
        Task<IEnumerable<OrderTable>> GetPendingPaymentOrdersAsync(DateTime cutoffTime);
        Task CancelOrderAsync(int orderId);
    }
}