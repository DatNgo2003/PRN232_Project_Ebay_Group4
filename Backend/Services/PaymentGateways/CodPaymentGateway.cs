namespace Backend.Services.PaymentGateways;

/// <summary>
/// Cổng thanh toán COD giả lập — chứng minh kiến trúc plug-in hoạt động
/// với nhiều provider khác nhau chứ không chỉ PayPal.
/// </summary>
public sealed class CodPaymentGateway : IPaymentGateway
{
    private readonly ITransactionLogger _txLogger;

    public string ProviderName => "COD";

    public CodPaymentGateway(ITransactionLogger txLogger)
    {
        _txLogger = txLogger;
    }

    public Task<PaymentInitiationResult> InitiateAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default)
    {
        var txId = _txLogger.StartTransaction("Payment.COD", "Initiate",
            new { request.OrderReference, request.Amount });

        var providerRef = $"COD-{request.OrderReference}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        _txLogger.LogSuccess(txId, "Payment.COD", "Initiate", new { providerRef });

        return Task.FromResult(new PaymentInitiationResult(true, providerRef, "PENDING_ON_DELIVERY"));
    }

    public Task<PaymentCaptureResult> CaptureAsync(
        string providerTransactionId,
        CancellationToken cancellationToken = default)
    {
        // COD chỉ được "capture" (thu tiền) khi shipper xác nhận giao thành công.
        var txId = _txLogger.StartTransaction("Payment.COD", "Capture", new { providerTransactionId });
        _txLogger.LogSuccess(txId, "Payment.COD", "Capture", new { providerTransactionId });

        return Task.FromResult(new PaymentCaptureResult(true, providerTransactionId, "COLLECTED", null, "VND"));
    }
}