namespace Backend.DTOs.Responses
{
    public class CouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public decimal DiscountPercent { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxUsage { get; set; }
        public int? UsedCount { get; set; }
    }
}