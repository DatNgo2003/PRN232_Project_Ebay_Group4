namespace Backend.Services.PaymentGateways;

public sealed record PaymentInitiationRequest(
    decimal Amount,
    string Currency,
    string OrderReference,
    string Description);

public sealed record PaymentInitiationResult(
    bool Success,
    string ProviderTransactionId,
    string Status);

public sealed record PaymentCaptureResult(
    bool Success,
    string ProviderTransactionId,
    string Status,
    decimal? Amount,
    string? Currency);