using Backend.DTOs.Requests;
using Backend.Models;
using Backend.Repositories.Interface;
using Backend.Services.Interface;

namespace Backend.Services.Implementation
{
    public class SellerToBuyerReviewService : ISellerToBuyerReviewService
    {
        private readonly ISellerToBuyerReviewRepository _repo;

        public SellerToBuyerReviewService(ISellerToBuyerReviewRepository repo)
        {
            _repo = repo;
        }

        public async Task<SellerToBuyerReview> CreateSellerReviewAsync(int sellerId, SellerReviewCreateDto reviewDto)
        {
            var order = await _repo.GetOrderDetailsAsync(reviewDto.OrderId);
            if (order == null)
            {
                throw new Exception("Order not found.");
            }

            var product = order.OrderItems.FirstOrDefault()?.Product;
            if (product == null)
            {
                throw new Exception("Product not found in order.");
            }

            if (product.SellerId != sellerId)
            {
                throw new Exception("You are not the seller for this order.");
            }

            if (await _repo.HasBeenReviewedAsync(reviewDto.OrderId, sellerId))
            {
                throw new Exception("You have already reviewed this order.");
            }

            var seller = await _repo.GetUserByIdAsync(sellerId);
            var buyer = order.Buyer;

            var newReview = new SellerToBuyerReview
            {
                SellerId = sellerId,
                SellerName = seller?.Username ?? "Unknown Seller",
                BuyerId = buyer.Id,
                BuyerName = buyer.Username ?? "Unknown Buyer",
                Comment = reviewDto.Comment,
                OrderId = reviewDto.OrderId,
                ProductName = product.Title ?? "Unknown Product",
                CreatedAt = DateTime.UtcNow
            };

            return await _repo.AddReviewAsync(newReview);
        }

        public async Task<IEnumerable<SellerToBuyerReview>> GetReviewsReceivedByBuyerAsync(int buyerId)
        {
            return await _repo.GetReviewsForBuyerAsync(buyerId);
        }
    }
}
