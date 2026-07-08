using Backend.Models;
using Backend.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/review")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly CloneEbayDbContext _context;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(CloneEbayDbContext context, ILogger<ReviewController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            _logger.LogInformation("===== DEBUG CLAIMS =====");
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }
            _logger.LogInformation("Is Authenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);
            _logger.LogInformation("========================");
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int reviewerId))
            {
                _logger.LogWarning("Không thể tìm thấy/phân tích UserId (NameIdentifier) từ token.");
                return Unauthorized("User ID not found or invalid in token.");
            }

            try
            {
                var hasPurchased = await _context.OrderTables
                    .AnyAsync(o => o.BuyerId == reviewerId &&
                                   o.Status == "Completed" &&
                                   o.OrderItems.Any(oi => oi.ProductId == dto.ProductId));

                if (!hasPurchased)
                {
                    _logger.LogWarning($"User {reviewerId} cố gắng review SP {dto.ProductId} mà chưa mua.");
                    return Forbid("You can only review items you have purchased and received.");
                }

                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ReviewerId == reviewerId && r.ProductId == dto.ProductId);

                if (existingReview != null)
                {
                    _logger.LogWarning($"User {reviewerId} cố gắng review SP {dto.ProductId} một lần nữa.");
                    return Conflict("You have already reviewed this item.");
                }

                var newReview = new Review
                {
                    ProductId = dto.ProductId,
                    ReviewerId = reviewerId,
                    Rating = dto.Rating, // ✅ FIX BUG #1: THÊM DÒNG NÀY
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(newReview);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {reviewerId} đã review thành công sản phẩm {dto.ProductId}");
                return Ok(new { success = true, message = "Review submitted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi nghiêm trọng khi User {reviewerId} tạo review cho SP {dto.ProductId}");
                return StatusCode(500, "An internal server error occurred while submitting your review.");
            }
        }
    }
}