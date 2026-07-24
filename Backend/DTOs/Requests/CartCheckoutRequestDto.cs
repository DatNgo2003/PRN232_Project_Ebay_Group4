namespace Backend.DTOs.Requests
{
    public class CartCheckoutRequestDto
    {
        public List<OrderItemRequestDto> Items { get; set; } = new();
        public string? PaymentMethod { get; set; }
        public int? AddressId { get; set; }
        public string? CouponCode { get; set; }
        public string? CarrierKey { get; set; }
    }

    public class PayPalCartCreateOrderRequestDto
    {
        public List<OrderItemRequestDto> Items { get; set; } = new();
        public int? AddressId { get; set; }
        public string? CouponCode { get; set; }
        public string? CarrierKey { get; set; }
    }
}