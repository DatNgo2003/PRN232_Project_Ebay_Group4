namespace Backend.DTOs.Requests
{
    public class QuickBuyCheckoutRequestDto
    {
        public int ProductId { get; set; }
        public string? PaymentMethod { get; set; }
        public int? AddressId { get; set; }
        public int Quantity { get; set; } = 1;
        public string? CouponCode { get; set; }
        public string? CarrierKey { get; set; }
    }
}