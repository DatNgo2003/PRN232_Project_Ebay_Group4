using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend.Controllers;

public sealed record ShippingWebhookPayload(int OrderId, string Status, string? TrackingNumber, DateTime? EventTime);

/// <summary>
/// Điểm "hook": nơi 1 đơn vị vận chuyển (thật hoặc giả lập) gọi ngược vào hệ thống
/// khi trạng thái vận đơn thay đổi, thay vì hệ thống phải chủ động poll.
/// Đây chính là điểm plug-in cho microservice vận chuyển bên ngoài.
/// Xác thực bằng secret key riêng (không dùng JWT vì caller là hệ thống ngoài, không phải user).
/// </summary>
[ApiController]
[Route("api/webhooks/shipping")]
[EnableRateLimiting("payment_shipping")] // >>> MỚI: giới hạn 20 request/phút/IP (chưa đăng nhập nên tính theo IP)
public sealed class ShippingWebhookController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ITransactionLogger _txLogger;
    private readonly IConfiguration _configuration;

    public ShippingWebhookController(
        IOrderService orderService,
        ITransactionLogger txLogger,
        IConfiguration configuration)
    {
        _orderService = orderService;
        _txLogger = txLogger;
        _configuration = configuration;
    }

    [HttpPost("status-update")]
    public async Task<IActionResult> HandleStatusUpdate(
        [FromHeader(Name = "X-Webhook-Secret")] string? secret,
        [FromBody] ShippingWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        var expectedSecret = _configuration["Shipping:WebhookSecret"];

        if (string.IsNullOrWhiteSpace(expectedSecret) || !string.Equals(secret, expectedSecret, StringComparison.Ordinal))
        {
            var authTxId = _txLogger.StartTransaction("Shipping.Webhook", "Auth", payload);
            _txLogger.LogFailure(authTxId, "Shipping.Webhook", "Auth",
                new UnauthorizedAccessException("Webhook secret không hợp lệ."), payload);
            return Unauthorized(new { message = "Invalid webhook secret." });
        }

        var txId = _txLogger.StartTransaction("Shipping.Webhook", "StatusUpdate", payload);

        try
        {
            var updated = await _orderService.UpdateShippingStatusAsync(payload.OrderId, payload.Status);

            if (!updated)
            {
                _txLogger.LogFailure(txId, "Shipping.Webhook", "StatusUpdate",
                    new InvalidOperationException("OrderId hoặc tracking number không hợp lệ."), payload);
                return NotFound(new { message = "Order không tồn tại hoặc tracking number không khớp." });
            }

            _txLogger.LogSuccess(txId, "Shipping.Webhook", "StatusUpdate", payload);
            return Ok(new { message = "Cập nhật trạng thái thành công.", transactionId = txId });
        }
        catch (Exception ex)
        {
            _txLogger.LogInterModuleError(txId, "Shipping.Webhook", "Order", "StatusUpdate", ex);
            return StatusCode(502, new { message = "Không thể cập nhật trạng thái đơn hàng." });
        }
    }
}