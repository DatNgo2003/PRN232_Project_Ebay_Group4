namespace Backend.DTOs.Requests;

public sealed class PayPalCreateOrderRequestDto
{
    public int ProductId { get; set; }
    public int? AddressId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? CouponCode { get; set; }
}

public sealed class PayPalCaptureOrderRequestDto
{
    public int OrderId { get; set; }
    public string PayPalOrderId { get; set; } = string.Empty;
}
