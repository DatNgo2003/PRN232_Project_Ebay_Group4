using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/seller-reviews")]
    [ApiController]
    public class SellerReviewsController : ControllerBase
    {
        private readonly ISellerToBuyerReviewService _reviewService;

        public SellerReviewsController(ISellerToBuyerReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> CreateSellerReview([FromBody] SellerReviewCreateDto reviewDto)
        {
            try
            {
                var sellerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(sellerIdStr))
                {
                    return Unauthorized("User not found.");
                }
                var sellerId = int.Parse(sellerIdStr);

                var newReview = await _reviewService.CreateSellerReviewAsync(sellerId, reviewDto);

                return Ok(newReview);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("received-as-buyer")]
        [Authorize(Roles = "buyer")]
        public async Task<IActionResult> GetReviewsReceivedByBuyer()
        {
            try
            {
                var buyerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(buyerIdStr))
                {
                    return Unauthorized("User not found.");
                }
                var buyerId = int.Parse(buyerIdStr);

                var reviews = await _reviewService.GetReviewsReceivedByBuyerAsync(buyerId);

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
