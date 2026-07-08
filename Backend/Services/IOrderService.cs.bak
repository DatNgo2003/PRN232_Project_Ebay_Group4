using Backend.DTOs.Responses;

namespace Backend.Services
{
    public interface IOrderService
    {
        Task<bool> CreateQuickBuyOrderAsync(string buyerUsername, int productId);
        Task<IEnumerable<PurchaseHistoryItemDto>> GetPurchaseHistoryAsync(string buyerUsername);
    }
}