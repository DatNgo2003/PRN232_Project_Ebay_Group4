using System;
using System.Collections.Generic;

namespace Backend.DTOs.Responses
{
    public class SellerSalesOrderDto
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? OrderStatus { get; set; }
        public decimal? OrderTotalPrice { get; set; }

        public int BuyerId { get; set; }
        public string? BuyerUsername { get; set; }

        public List<SellerSalesItemDto> Items { get; set; } = new List<SellerSalesItemDto>();

        public bool HasBuyerFeedback { get; set; } = false;
        public int? BuyerFeedbackId { get; set; }
        public decimal? BuyerFeedbackRating { get; set; }

        // Shipping information
        public string? ShippingCarrier { get; set; }
        public string? TrackingNumber { get; set; }
        public string? ShippingStatus { get; set; }
        public DateTime? EstimatedArrival { get; set; }
        public decimal? ShippingFee { get; set; }

        // Address info
        public string? ShippingCity { get; set; }
        public string? ShippingCountry { get; set; }
    }
}