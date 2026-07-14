namespace Backend.DTOs.Responses
{
    public class QuickBuyCheckoutResponseDto
    {
        public int OrderId { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string PaymentStatus { get; set; } = "";

        public int AddressId { get; set; }
        public string ShippingDestination { get; set; } = ""; 

        public string TrackingNumber { get; set; } = "";
        public string ShippingStatus { get; set; } = "Preparing";
        public DateTime EstimatedArrival { get; set; }

        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? AppliedCoupon { get; set; }
    }
}
