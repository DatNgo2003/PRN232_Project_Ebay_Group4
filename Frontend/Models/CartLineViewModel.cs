namespace Frontend.Models
{
    public class CartLineViewModel
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = "";
        public string? Image { get; set; }
        public string? SellerName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}