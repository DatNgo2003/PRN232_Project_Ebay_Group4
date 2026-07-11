using Backend.DTOs.Responses;

namespace Backend.Services
{
    public interface IOrderService
    {
        Task<QuickBuyCheckoutResponseDto?> CreateQuickBuyOrderAsync(
            string buyerUsername,
            int productId,
            string? paymentMethod,
            string? shippingRegion);

        Task<IEnumerable<PurchaseHistoryItemDto>> GetPurchaseHistoryAsync(string buyerUsername);

        Task<IEnumerable<SellerSalesOrderDto>> GetSalesHistoryAsync(string sellerUsername);

        /// <summary>
        /// Cập nhật trạng thái giao hàng, đồng thời gửi email thông báo cho buyer.
        /// newShippingStatus: "Delivered" | "Failed" | "Shipping" | v.v.
        /// </summary>
        Task<bool> UpdateShippingStatusAsync(int orderId, string newShippingStatus);
    }
}

