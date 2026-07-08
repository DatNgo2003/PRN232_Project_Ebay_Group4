using System;
using System.Collections.Generic;

namespace Backend.DTOs.Responses
{
    public class SellerSalesOrderDto
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? OrderStatus { get; set; }
        public decimal? OrderTotalPrice { get; set; } // Tổng tiền của *cả* đơn hàng

        public int BuyerId { get; set; }
        public string? BuyerUsername { get; set; }

        public List<SellerSalesItemDto> Items { get; set; } = new List<SellerSalesItemDto>();

        public bool HasBuyerFeedback { get; set; } = false; // Mặc định là false
        public int? BuyerFeedbackId { get; set; }
        public decimal? BuyerFeedbackRating { get; set; }
    }
}