using Backend.Services;
using Backend.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private string GetUsernameFromToken()
        {
            return User.Identity?.Name ?? throw new InvalidOperationException("User is not authenticated.");
        }

        [HttpPost("quick-buy")]
        public async Task<IActionResult> QuickBuy(
            [FromQuery] int productId,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] QuickBuyCheckoutRequestDto? request)
        {
            try
            {
                var username = GetUsernameFromToken();
                var requestedProductId = request?.ProductId > 0 ? request.ProductId : productId;
                if (requestedProductId <= 0)
                {
                    return BadRequest(new { message = "ProductId is required." });
                }

                var checkout = await _orderService.CreateQuickBuyOrderAsync(
                    username,
                    requestedProductId,
                    request?.PaymentMethod,
                    request?.ShippingRegion);

                if (checkout == null)
                {
                    return BadRequest(new { message = "Failed to create order. Product or user not found." });
                }

                return Ok(new { message = "Order created successfully.", checkout });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyPurchaseHistory()
        {
            try
            {
                var username = GetUsernameFromToken();
                var history = await _orderService.GetPurchaseHistoryAsync(username);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("seller/my-sales")]
        [Authorize(Roles = "seller, supporter")] // Chỉ cho phép Seller
        public async Task<IActionResult> GetMySalesHistory()
        {
            try
            {
                // Dùng hàm có sẵn để lấy username từ token
                var username = GetUsernameFromToken();

                // Gọi service method mới
                var history = await _orderService.GetSalesHistoryAsync(username);

                return Ok(history);
            }
            catch (InvalidOperationException ex) // Lỗi từ GetUsernameFromToken
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log lỗi ex ở đây
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ: " + ex.Message });
            }
        }
    }
}
