namespace Backend.Services.PaymentGateways;

/// <summary>
/// Adapter: bọc IPayPalClient hiện có thành 1 IPaymentGateway "cắm được".
/// Toàn bộ logic gọi PayPal thật vẫn nằm trong PayPalClient, không đổi.
/// </summary>
public sealed class PayPalPaymentGateway : IPaymentGateway
{
    private readonly IPayPalClient _payPalClient;
    private readonly ITransactionLogger _txLogger;

    public string ProviderName => "PayPal";

    public PayPalPaymentGateway(IPayPalClient payPalClient, ITransactionLogger txLogger)
    {
        _payPalClient = payPalClient;
        _txLogger = txLogger;
    }

    public async Task<PaymentInitiationResult> InitiateAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default)
    {
        var txId = _txLogger.StartTransaction("Payment.PayPal", "Initiate",
            new { request.OrderReference, request.Amount, request.Currency });

        try
        {
            var order = await _payPalClient.CreateOrderAsync(
                request.Amount, request.OrderReference, request.Description, cancellationToken);

            _txLogger.LogSuccess(txId, "Payment.PayPal", "Initiate", new { order.Id, order.Status });
            return new PaymentInitiationResult(true, order.Id, order.Status);
        }
        catch (Exception ex)
        {
            // Lỗi giao tiếp giữa module Payment và PayPal API (bên ngoài)
            _txLogger.LogInterModuleError(txId, "Payment.PayPal", "PayPal API", "Initiate", ex);
            return new PaymentInitiationResult(false, string.Empty, "FAILED");
        }
    }

    public async Task<PaymentCaptureResult> CaptureAsync(
        string providerTransactionId,
        CancellationToken cancellationToken = default)
    {
        var txId = _txLogger.StartTransaction("Payment.PayPal", "Capture", new { providerTransactionId });

        try
        {
            var capture = await _payPalClient.CaptureOrderAsync(providerTransactionId, cancellationToken);
            var success = string.Equals(capture.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase);

            if (success)
                _txLogger.LogSuccess(txId, "Payment.PayPal", "Capture", new { capture.Id, capture.Status });
            else
                _txLogger.LogFailure(txId, "Payment.PayPal", "Capture",
                    new InvalidOperationException($"PayPal capture status = {capture.Status}"), new { capture.Id });

            return new PaymentCaptureResult(success, capture.Id, capture.Status, capture.Amount, capture.CurrencyCode);
        }
        catch (Exception ex)
        {
            _txLogger.LogInterModuleError(txId, "Payment.PayPal", "PayPal API", "Capture", ex);
            return new PaymentCaptureResult(false, providerTransactionId, "FAILED", null, null);
        }
    }
}