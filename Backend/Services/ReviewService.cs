using Backend.DTOs.Requests;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;

        public ReviewService(
            IReviewRepository reviewRepository,
            IUserRepository userRepository)
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
        }

        public async Task<(bool Success, string Message)> CreateReviewAsync(CreateReviewDto dto, string reviewerUsername)
        {
            var user = await _userRepository.GetByUsernameAsync(reviewerUsername);
            if (user == null)
            {
                return (false, "User not found.");
            }
            var reviewerId = user.Id;

            bool alreadyReviewed = await _reviewRepository.HasUserReviewedProductAsync(reviewerId, dto.ProductId);

            if (alreadyReviewed)
            {
                return (false, "You have already reviewed this product.");
            }

            var review = new Review
            {
                ProductId = dto.ProductId,
                ReviewerId = reviewerId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddReviewAsync(review);

            return (true, "Review added successfully.");
        }
    }
}