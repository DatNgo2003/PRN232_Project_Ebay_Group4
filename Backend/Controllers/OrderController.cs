using Backend.Services;
using Backend.DTOs.Requests;
using Backend.Exceptions;
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
        private readonly IOrderPricingService _orderPricingService; 

        public OrderController(IOrderService orderService, IOrderPricingService orderPricingService)
        {
            _orderService = orderService;
            _orderPricingService = orderPricingService; 
        }

        /// <summary>
        /// Tính trước tổng tiền đơn hàng (SubTotal, DiscountAmount, ShippingFee, Total)
        /// trước khi người dùng bấm mua thật. Không tạo đơn hàng, không lưu DB.
        /// </summary>
        [HttpPost("calculate-total")]
        public async Task<IActionResult> CalculateTotal([FromBody] Backend.DTOs.Requests.CalculateOrderDto dto)
        {
            try
            {
                var result = await _orderPricingService.CalculateAsync(dto);
                return Ok(result);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
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
                    request?.AddressId,       
                    request?.Quantity ?? 1,
                    request?.CouponCode);

                if (checkout == null)
                {
                    return BadRequest(new { message = "Failed to create order. Product or user not found." });
                }

                return Ok(new { message = "Order created successfully.", checkout });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { message = ex.Message });
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
        [Authorize(Roles = "seller, supporter")]
        public async Task<IActionResult> GetMySalesHistory()
        {
            try
            {
                var username = GetUsernameFromToken();
                var history = await _orderService.GetSalesHistoryAsync(username);
                return Ok(history);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ: " + ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái giao hàng của đơn hàng.
        /// Khi status = "Delivered" hoặc "Failed", buyer sẽ nhận email thông báo tự động.
        /// Chỉ seller và supporter được phép gọi API này.
        /// </summary>
        [HttpPut("{orderId}/shipping-status")]
        [Authorize(Roles = "seller, supporter, admin")]
        public async Task<IActionResult> UpdateShippingStatus(
            int orderId,
            [FromBody] UpdateShippingStatusRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request?.Status))
            {
                return BadRequest(new { message = "Trường 'status' không được để trống." });
            }

            var validStatuses = new[] { "Preparing", "Shipping", "Delivered", "Failed" };
            if (!validStatuses.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = $"Trạng thái không hợp lệ. Các giá trị cho phép: {string.Join(", ", validStatuses)}"
                });
            }

            try
            {
                var updated = await _orderService.UpdateShippingStatusAsync(orderId, request.Status);
                if (!updated)
                {
                    return NotFound(new { message = $"Không tìm thấy đơn hàng #{orderId}." });
                }

                return Ok(new
                {
                    message = $"Cập nhật trạng thái giao hàng thành '{request.Status}' thành công.",
                    orderId,
                    shippingStatus = request.Status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ: " + ex.Message });
            }
        }
    }
}