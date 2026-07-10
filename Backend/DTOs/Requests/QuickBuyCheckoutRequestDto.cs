namespace Backend.DTOs.Requests
{
    public class QuickBuyCheckoutRequestDto
    {
        public int ProductId { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ShippingRegion { get; set; }
    }
}
