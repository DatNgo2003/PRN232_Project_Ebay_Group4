using Backend.DTOs.Requests;
using Backend.Models;

namespace Backend.Services.Interface
{
    public interface ISellerToBuyerReviewService
    {
        Task<SellerToBuyerReview> CreateSellerReviewAsync(int sellerId, SellerReviewCreateDto reviewDto);
        Task<IEnumerable<SellerToBuyerReview>> GetReviewsReceivedByBuyerAsync(int buyerId);
    }
}
