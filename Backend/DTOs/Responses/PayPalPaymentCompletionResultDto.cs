namespace Backend.DTOs.Responses;

public sealed class PayPalPaymentCompletionResultDto
{
    public int OrderId { get; set; }
    public string PayPalOrderId { get; set; } = string.Empty;
    public string PayPalCaptureId { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
}
