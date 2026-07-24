using Backend.Models;

namespace Backend.Repositories
{
    // >>> MỚI: input cho 1 dòng sản phẩm khi tạo đơn hàng nhiều sản phẩm (checkout từ giỏ hàng)
    public sealed record CartOrderItemInput(int ProductId, int Quantity, decimal UnitPrice);

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
            string shippingCarrier,
            string shippingStatus,
            string trackingNumber,
            DateTime estimatedArrival,
            int quantity,
            decimal subTotal,
            decimal discountAmount,
            decimal totalAmount,
            int? couponId,
            bool confirmInventoryImmediately);

        // >>> MỚI: tạo 1 đơn hàng gồm NHIỀU sản phẩm (checkout từ giỏ hàng)
        Task<OrderTable> CreateMultiItemOrderAsync(
            int buyerId,
            List<CartOrderItemInput> items,
            decimal shippingFee,
            string paymentMethod,
            string paymentStatus,
            string orderStatus,
            int addressId,
            string trackingNumber,
            DateTime estimatedArrival,
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
        Task<bool> FailPayPalPaymentAsync(int orderId, int buyerId, string paypalOrderId, string failureStatus);
    }
}
