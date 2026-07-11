using System.Collections.Generic;

namespace Backend.DTOs.Requests
{
    public class CalculateOrderDto
    {
        public List<OrderItemRequestDto> Items { get; set; } = new();
        public int AddressId { get; set; }
        public string? CouponCode { get; set; }
    }

    public class OrderItemRequestDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}