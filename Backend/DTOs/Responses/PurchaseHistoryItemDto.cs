using System;

namespace Backend.DTOs.Responses
{
    public class PurchaseHistoryItemDto
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string? ProductTitle { get; set; }
        public string? ProductImage { get; set; }
        public decimal? UnitPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public string FeedbackState { get; set; } = "N/A";
        public string? OrderStatus { get; set; }
        public string? SellerUsername { get; set; }
        public int? SellerId { get; set; }
    }
}