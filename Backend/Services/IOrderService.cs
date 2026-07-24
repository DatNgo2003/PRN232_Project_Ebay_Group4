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

        Task<bool> AttachPayPalOrderAsync(
            string buyerUsername,
            int orderId,
            string paypalOrderId);

        Task<bool> IsPayPalOrderOwnedAsync(
            string buyerUsername,
            int orderId,
            string paypalOrderId);

        Task<PayPalPaymentCompletionResultDto?> CompletePayPalPaymentAsync(
            string buyerUsername,
            int orderId,
            string paypalOrderId,
            string paypalCaptureId,
            decimal capturedAmount,
            string currency);

        Task<bool> FailPayPalPaymentAsync(
            string buyerUsername,
            int orderId,
            string paypalOrderId,
            string failureStatus);

        Task CancelOrderAsync(int orderId);
    }
}
