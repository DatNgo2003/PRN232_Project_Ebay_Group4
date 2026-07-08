namespace Frontend.Models
{
    public class SellerSalesItemDto
    {
        public int ProductId { get; set; }
        public string? ProductTitle { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
    }
}