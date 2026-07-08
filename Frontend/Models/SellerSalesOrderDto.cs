using System;
using System.Collections.Generic;

namespace Frontend.Models
{
    public class SellerSalesOrderDto
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? OrderStatus { get; set; }
        public decimal? OrderTotalPrice { get; set; }

        public int BuyerId { get; set; }
        public string? BuyerUsername { get; set; }

        public bool HasBuyerFeedback { get; set; }
        public int? BuyerFeedbackId { get; set; }
        public decimal? BuyerFeedbackRating { get; set; }

        public List<SellerSalesItemDto> Items { get; set; } = new List<SellerSalesItemDto>();
    }
}