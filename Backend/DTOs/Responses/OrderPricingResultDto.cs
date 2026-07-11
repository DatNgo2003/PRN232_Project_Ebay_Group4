namespace Backend.DTOs.Responses
{
    public class OrderPricingResultDto
    {
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        public string? AppliedCoupon { get; set; }
        public int? CouponId { get; set; }
    }
}