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
            int addressId,
            string trackingNumber,
            DateTime estimatedArrival,
            int quantity,
            decimal subTotal,
            decimal discountAmount,
            decimal totalAmount,
            int? couponId)
        {
            var newOrder = new OrderTable
            {
                BuyerId = buyerId,
                AddressId = addressId,
                OrderDate = DateTime.UtcNow,
                TotalPrice = totalAmount,
                Status = orderStatus,

                SubTotal = subTotal,
                ShippingFee = shippingFee,
                DiscountAmount = discountAmount,
                CouponId = couponId
            };

            _context.OrderTables.Add(newOrder);

            var newOrderItem = new OrderItem
            {
                Order = newOrder,
                ProductId = productId,
                Quantity = quantity,
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

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId);
            var carrierLabel = address != null
                ? $"MockExpress - {address.City ?? address.Country ?? "N/A"}"
                : "MockExpress";

            var shippingInfo = new ShippingInfo
            {
                Order = newOrder,
                Carrier = carrierLabel,
                TrackingNumber = trackingNumber,
                Status = "Preparing",
                EstimatedArrival = estimatedArrival
            };

            _context.ShippingInfos.Add(shippingInfo);

            await _context.SaveChangesAsync();
            return newOrder;
        }

        // >>> MỚI: tạo 1 đơn hàng gồm NHIỀU sản phẩm (checkout từ giỏ hàng) —
        // 1 Order, nhiều OrderItem, 1 Payment, 1 ShippingInfo dùng chung cho cả đơn.
        public async Task<OrderTable> CreateMultiItemOrderAsync(
            int buyerId,
            List<CartOrderItemInput> items,
            decimal shippingFee,
            string paymentMethod,
            string paymentStatus,
            string orderStatus,
            int addressId,
            string trackingNumber,
            DateTime estimatedArrival,
            decimal subTotal,
            decimal discountAmount,
            decimal totalAmount,
            int? couponId)
        {
            var newOrder = new OrderTable
            {
                BuyerId = buyerId,
                AddressId = addressId,
                OrderDate = DateTime.UtcNow,
                TotalPrice = totalAmount,
                Status = orderStatus,

                SubTotal = subTotal,
                ShippingFee = shippingFee,
                DiscountAmount = discountAmount,
                CouponId = couponId
            };

            _context.OrderTables.Add(newOrder);

            foreach (var item in items)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    Order = newOrder,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

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

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId);
            var carrierLabel = address != null
                ? $"MockExpress - {address.City ?? address.Country ?? "N/A"}"
                : "MockExpress";

            var shippingInfo = new ShippingInfo
            {
                Order = newOrder,
                Carrier = carrierLabel,
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
                    .ThenInclude(o => o.Payments)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.ShippingInfos)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Feedbacks)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Disputes)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.ReturnRequests)
                .Where(oi => oi.Order != null && oi.Order.BuyerId == buyerId)
                .OrderByDescending(oi => oi.Order!.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsBySellerIdAsync(int sellerId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Buyer)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Feedbacks)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.ShippingInfos)
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.Address)
                .Where(oi => oi.Product != null && oi.Product.SellerId == sellerId)
                .OrderByDescending(oi => oi.Order!.OrderDate)
                .ToListAsync();
        }

        public async Task<OrderTable?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _context.OrderTables
                .Include(o => o.Buyer)
                .Include(o => o.ShippingInfos)
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

            var shippingInfo = order.ShippingInfos
                .OrderByDescending(s => s.EstimatedArrival)
                .FirstOrDefault();

            if (shippingInfo != null)
            {
                shippingInfo.Status = newShippingStatus;
            }

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
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Status == "Pending" && o.OrderDate < cutoffTime
                    && o.Payments.Any(p => p.Status == "Pending"))
                .ToListAsync();
        }

        public async Task CancelOrderAsync(int orderId)
        {
            var order = await _context.OrderTables
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return;

            order.Status = "Cancelled";
            foreach (var payment in order.Payments)
            {
                if (payment.Status == "Pending")
                {
                    payment.Status = "Cancelled";
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}