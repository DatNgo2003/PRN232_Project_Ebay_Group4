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

        // ─── NEW METHODS ────────────────────────────────────────────────────────

        public async Task<OrderTable?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _context.OrderTables
                .Include(o => o.Buyer)
                .Include(o => o.ShippingInfos)
                .Include(o => o.Payments)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task UpdateShippingStatusAsync(int orderId, string newShippingStatus)
        {
            var order = await _context.OrderTables
                .Include(o => o.ShippingInfos)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return;

            // Cập nhật tất cả ShippingInfo của order
            foreach (var shipping in order.ShippingInfos)
            {
                shipping.Status = newShippingStatus;
            }

            // Đồng bộ OrderTable.Status theo trạng thái giao hàng
            if (newShippingStatus.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
            {
                order.Status = "Completed";
            }
            else if (newShippingStatus.Equals("Failed", StringComparison.OrdinalIgnoreCase))
            {
                order.Status = "Failed";
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrderTable>> GetPendingPaymentOrdersAsync(DateTime cutoffTime)
        {
            return await _context.OrderTables
                .Include(o => o.Buyer)
                .Include(o => o.Payments)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o =>
                    o.Status == "Pending" &&
                    o.OrderDate < cutoffTime &&
                    o.Payments.Any(p => p.Status == "Pending"))
                .ToListAsync();
        }

        public async Task CancelOrderAsync(int orderId)
        {
            var order = await _context.OrderTables
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return;

            order.Status = "Cancelled";

            foreach (var payment in order.Payments.Where(p => p.Status == "Pending"))
            {
                payment.Status = "Cancelled";
            }

            await _context.SaveChangesAsync();
        }
    }
}

