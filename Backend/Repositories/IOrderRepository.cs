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

        /// <summary>
        /// Lấy order kèm đầy đủ thông tin: Buyer, ShippingInfos, Payments, OrderItems, Products
        /// </summary>
        Task<OrderTable?> GetOrderWithDetailsAsync(int orderId);

        /// <summary>
        /// Cập nhật trạng thái giao hàng của ShippingInfo, đồng thời cập nhật OrderTable.Status nếu cần
        /// </summary>
        Task UpdateShippingStatusAsync(int orderId, string newShippingStatus);

        /// <summary>
        /// Lấy các đơn hàng Pending chưa thanh toán, tạo trước cutoffTime (để auto-cancel)
        /// </summary>
        Task<IEnumerable<OrderTable>> GetPendingPaymentOrdersAsync(DateTime cutoffTime);

        /// <summary>
        /// Huỷ đơn hàng: set OrderTable.Status = "Cancelled", Payment.Status = "Cancelled"
        /// </summary>
        Task CancelOrderAsync(int orderId);
    }
}

