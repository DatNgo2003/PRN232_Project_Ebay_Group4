using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services
{
    public interface IAuctionService
    {
        Task<BidResponseDto?> PlaceBidAsync(string bidderUsername, PlaceBidDto dto);
        Task<IEnumerable<BidResponseDto>> GetBidsByProductAsync(int productId);
        Task<BidResponseDto?> GetHighestBidAsync(int productId);
        Task<IEnumerable<AuctionListItemDto>> GetMyAuctionsAsync(string username);
        Task<IEnumerable<AuctionListItemDto>> GetActiveAuctionsAsync();
        Task<bool> FinalizeAuctionAsync(int productId);
    }
}
