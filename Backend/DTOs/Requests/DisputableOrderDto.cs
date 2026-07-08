namespace Backend.DTOs.Requests
{
    public class DisputableOrderDto
    {
        public int OrderId { get; set; }
        public string PrimaryProductName { get; set; }
        public string SellerName { get; set; }
        public DateTime? OrderDate { get; set; }
        public string OrderStatus { get; set; }
    }
}
