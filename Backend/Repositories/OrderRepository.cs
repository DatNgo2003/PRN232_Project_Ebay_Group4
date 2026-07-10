using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly CloneEbayDbContext _context;

        public OrderRepository(CloneEbayDbContext context)
        {
            _context = context;
        }

        public async Task<OrderTable> CreateSimpleOrderAsync(
            int buyerId,
            int productId,
            decimal unitPrice,
            decimal shippingFee,
            string paymentMethod,
            string paymentStatus,
            string orderStatus,
            string shippingRegion,
            string trackingNumber,
            DateTime estimatedArrival)
        {
            var totalAmount = unitPrice + shippingFee;
            var newOrder = new OrderTable
            {
                BuyerId = buyerId,
                OrderDate = DateTime.UtcNow,
                TotalPrice = totalAmount,
                Status = orderStatus
            };

            _context.OrderTables.Add(newOrder);

            var newOrderItem = new OrderItem
            {
                Order = newOrder,
                ProductId = productId,
                Quantity = 1,
                UnitPrice = unitPrice
            };

            _context.OrderItems.Add(newOrderItem);

            var payment = new Payment
            {
                Order = newOrder,
                UserId = buyerId,
                Amount = totalAmount,
                Method = paymentMethod,
                Status = paymentStatus,
                PaidAt = paymentStatus == "Paid" ? DateTime.UtcNow : null
            };

            _context.Payments.Add(payment);

            var shippingInfo = new ShippingInfo
            {
                Order = newOrder,
                Carrier = $"MockExpress - {shippingRegion}",
                TrackingNumber = trackingNumber,
                Status = "Preparing",
                EstimatedArrival = estimatedArrival
            };

            _context.ShippingInfos.Add(shippingInfo);

            await _context.SaveChangesAsync();
            return newOrder;
        }

        public async Task<IEnumerable<OrderItem>> GetPurchaseHistoryAsync(int buyerId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Seller)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Feedbacks)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Disputes)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.ReturnRequests)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Payments)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.ShippingInfos)
                .Where(oi => oi.Order.BuyerId == buyerId)
                .OrderByDescending(oi => oi.Order.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsBySellerIdAsync(int sellerId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Buyer)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Feedbacks)
                .Include(oi => oi.Product)
                .Where(oi => oi.Product != null && oi.Product.SellerId == sellerId)
                .OrderByDescending(oi => oi.Order.OrderDate)
                .ToListAsync();
        }
    }
}
