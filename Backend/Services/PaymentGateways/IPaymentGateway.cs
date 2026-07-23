namespace Backend.Services.PaymentGateways;

/// <summary>
/// Điểm plug-in cho module thanh toán. Muốn thêm cổng thanh toán mới
/// (VNPay, Momo, ...) chỉ cần viết class implement interface này và
/// đăng ký thêm trong Program.cs — không cần sửa Controller/OrderService.
/// </summary>
public interface IPaymentGateway
{
    /// Tên định danh dùng để resolve qua IPaymentGatewayFactory (vd: "PayPal", "COD").
    string ProviderName { get; }

    Task<PaymentInitiationResult> InitiateAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentCaptureResult> CaptureAsync(
        string providerTransactionId,
        CancellationToken cancellationToken = default);
}