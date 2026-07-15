using Backend.DTOs.Requests;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Backend.Configuration;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/paypal")]
[Authorize]
public sealed class PayPalController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPayPalClient _paypalClient;
    private readonly PayPalOptions _paypalOptions;
    private readonly ILogger<PayPalController> _logger;

    public PayPalController(
        IOrderService orderService,
        IPayPalClient paypalClient,
        IOptions<PayPalOptions> paypalOptions,
        ILogger<PayPalController> logger)
    {
        _orderService = orderService;
        _paypalClient = paypalClient;
        _paypalOptions = paypalOptions.Value;
        _logger = logger;
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder(
        [FromBody] PayPalCreateOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.ProductId <= 0)
            return BadRequest(new { message = "ProductId is required." });

        try
        {
            var username = GetUsernameFromToken();
            var checkout = await _orderService.CreateQuickBuyOrderAsync(
                username,
                request.ProductId,
                "PayPal",
                request.AddressId,
                request.Quantity,
                request.CouponCode);

            if (checkout == null)
                return BadRequest(new { message = "Unable to prepare the order." });

            PayPalOrderResult paypalOrder;
            try
            {
                paypalOrder = await _paypalClient.CreateOrderAsync(
                    checkout.TotalAmount,
                    checkout.OrderId.ToString(),
                    $"Order #{checkout.OrderId}",
                    cancellationToken);
            }
            catch
            {
                await _orderService.CancelOrderAsync(checkout.OrderId);
                throw;
            }

            var attached = await _orderService.AttachPayPalOrderAsync(
                username,
                checkout.OrderId,
                paypalOrder.Id);

            if (!attached)
            {
                await _orderService.CancelOrderAsync(checkout.OrderId);
                return BadRequest(new { message = "Unable to link the PayPal order." });
            }

            return Ok(new
            {
                id = paypalOrder.Id,
                orderId = checkout.OrderId,
                status = paypalOrder.Status,
                amount = checkout.TotalAmount,
                currency = _paypalOptions.Currency
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create PayPal order for product {ProductId}", request.ProductId);
            return StatusCode(502, new { message = "Không thể tạo thanh toán PayPal. Vui lòng thử lại." });
        }
    }

    [HttpPost("capture-order")]
    public async Task<IActionResult> CaptureOrder(
        [FromBody] PayPalCaptureOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.OrderId <= 0 || string.IsNullOrWhiteSpace(request.PayPalOrderId))
            return BadRequest(new { message = "OrderId and PayPalOrderId are required." });

        try
        {
            var username = GetUsernameFromToken();
            var owned = await _orderService.IsPayPalOrderOwnedAsync(
                username,
                request.OrderId,
                request.PayPalOrderId);

            if (!owned)
                return NotFound(new { message = "PayPal order không tồn tại hoặc không thuộc tài khoản này." });

            var capture = await _paypalClient.CaptureOrderAsync(
                request.PayPalOrderId,
                cancellationToken);

            if (!string.Equals(capture.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "PayPal chưa hoàn tất thanh toán.",
                    status = capture.Status
                });
            }

            if (capture.Amount == null ||
                string.IsNullOrWhiteSpace(capture.Id) ||
                !string.Equals(capture.CurrencyCode, _paypalOptions.Currency, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "PayPal không trả về số tiền capture hợp lệ." });

            var completed = await _orderService.CompletePayPalPaymentAsync(
                username,
                request.OrderId,
                request.PayPalOrderId,
                capture.Id,
                capture.Amount.Value,
                capture.CurrencyCode!);

            if (completed == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng nội bộ để cập nhật." });

            return Ok(completed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to capture PayPal order {PayPalOrderId}", request.PayPalOrderId);
            return StatusCode(502, new { message = "Không thể hoàn tất thanh toán PayPal. Vui lòng thử lại." });
        }
    }

    private string GetUsernameFromToken()
    {
        return User.Identity?.Name
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? throw new InvalidOperationException("User is not authenticated.");
    }
}
