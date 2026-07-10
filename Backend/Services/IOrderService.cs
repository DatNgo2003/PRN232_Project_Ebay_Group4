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
    }
}
