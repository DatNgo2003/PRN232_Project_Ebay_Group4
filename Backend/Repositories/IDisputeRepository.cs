using Backend.DTOs.Dispute;
using Backend.Models;
using Microsoft.AspNetCore.OData.Query;

namespace Backend.Repositories
{
    public interface IDisputeRepository
    {
        Task<List<DisputeListItemDto>> GetDisputesByBuyerAsync(int buyerId);
        Task<List<DisputeListItemDto>> GetDisputesBySellerAsync(int sellerId);
        Task<List<DisputeListItemDto>> GetDisputesForSupporterAsync();
        void RespondDispute(RespondDisputeDto respond);
        Task AutoEscalateDisputesAsync(int daysToEscalate);

    }
}
