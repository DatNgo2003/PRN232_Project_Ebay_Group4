using Backend.Filters;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

public sealed record PaymentWebhookPayload(
    string ProviderTransactionId,
    string OrderReference,
    string Status,
    decimal? Amount,
    string? Currency,
    DateTime? EventTime);

/// <summary>
/// Điểm "hook" giả lập: nơi cổng thanh toán (PayPal/COD giả lập) gọi ngược vào
/// hệ thống khi trạng thái giao dịch thay đổi, thay vì hệ thống phải chủ động
/// poll trạng thái. Đây là điểm plug-in cho module thanh toán bên ngoài.
///
/// Xác thực bằng secret key riêng (Payment:WebhookSecret trong appsettings.json)
/// — KHÔNG dùng JWT vì caller là hệ thống thanh toán ngoài, không phải user.
/// Việc kiểm tra token/secret key do VerifyWebhookSecretAttribute đảm nhiệm,
/// request chỉ tới được action bên dưới nếu đã xác thực hợp lệ.
/// </summary>
[ApiController]
[Route("api/webhooks/payment")]
[VerifyWebhookSignature("Payment:WebhookSecret", "Payment.Webhook")]
public sealed class PaymentWebhookController : ControllerBase
{
    private readonly ITransactionLogger _txLogger;

    public PaymentWebhookController(ITransactionLogger txLogger)
    {
        _txLogger = txLogger;
    }

    [HttpPost("status-update")]
    public IActionResult HandleStatusUpdate([FromBody] PaymentWebhookPayload payload)
    {
        // Đến được đây nghĩa là auth token/secret key đã hợp lệ.
        var txId = _txLogger.StartTransaction("Payment.Webhook", "StatusUpdate", payload);
        _txLogger.LogSuccess(txId, "Payment.Webhook", "StatusUpdate", payload);

        return Ok(new
        {
            message = "Webhook đã được xác thực và ghi nhận.",
            transactionId = txId
        });

        // Ghi chú: nối logic cập nhật đơn hàng nội bộ (nếu cần) thì gọi IOrderService
        // ngay tại đây, dùng payload.OrderReference — không thuộc phạm vi yêu cầu này.
    }
}