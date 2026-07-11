using Backend.DTOs.Responses;

namespace Backend.Services
{
    public interface IOrderService
    {
        Task<QuickBuyCheckoutResponseDto?> CreateQuickBuyOrderAsync(
            string buyerUsername,
            int productId,
            string? paymentMethod,
            int? addressId,              
            int quantity = 1,
            string? couponCode = null);

        Task<IEnumerable<PurchaseHistoryItemDto>> GetPurchaseHistoryAsync(string buyerUsername);

        Task<IEnumerable<SellerSalesOrderDto>> GetSalesHistoryAsync(string sellerUsername);

        Task<bool> UpdateShippingStatusAsync(int orderId, string newShippingStatus);
    }
}