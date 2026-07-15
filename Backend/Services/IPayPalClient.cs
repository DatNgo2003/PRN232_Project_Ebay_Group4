namespace Backend.Services;

public interface IPayPalClient
{
    Task<PayPalOrderResult> CreateOrderAsync(
        decimal amount,
        string referenceId,
        string description,
        CancellationToken cancellationToken = default);

    Task<PayPalCaptureResult> CaptureOrderAsync(
        string paypalOrderId,
        CancellationToken cancellationToken = default);
}

public sealed record PayPalOrderResult(string Id, string Status);

public sealed record PayPalCaptureResult(
    string Id,
    string Status,
    string? CurrencyCode,
    decimal? Amount);
