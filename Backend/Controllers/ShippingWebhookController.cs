using Backend.Filters;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

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
[VerifyWebhookSignature("Shipping:WebhookSecret", "Shipping.Webhook")]
public sealed class ShippingWebhookController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ITransactionLogger _txLogger;

    // KHÔNG cần IConfiguration nữa vì attribute đã lo việc verify secret
    public ShippingWebhookController(
        IOrderService orderService,
        ITransactionLogger txLogger)
    {
        _orderService = orderService;
        _txLogger = txLogger;
    }

    [HttpPost("status-update")]
    public async Task<IActionResult> HandleStatusUpdate(
        [FromBody] ShippingWebhookPayload payload,   // bỏ tham số [FromHeader] secret
        CancellationToken cancellationToken)
    {
        // Bỏ toàn bộ khối kiểm tra secret cũ — attribute đã xử lý trước khi vào đây

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
