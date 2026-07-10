namespace Backend.DTOs.Responses
{
    public class QuickBuyCheckoutResponseDto
    {
        public int OrderId { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string ShippingRegion { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public DateTime EstimatedArrival { get; set; }
    }
}
