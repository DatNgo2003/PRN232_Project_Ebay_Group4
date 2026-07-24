using Backend.Models;
using Backend.Exceptions;
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
            string shippingCarrier,
            string shippingStatus,
            string trackingNumber,
            DateTime estimatedArrival,
            int quantity,
            decimal subTotal,
            decimal discountAmount,
            decimal totalAmount,
            int? couponId,
            bool confirmInventoryImmediately)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var reservedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE [Inventory]
                SET [quantity] = [quantity] - {quantity},
                    [lastUpdated] = {DateTime.UtcNow}
                WHERE [productId] = {productId}
                  AND ISNULL([quantity], 0) >= {quantity};");

                if (reservedRows != 1)
                {
                    await transaction.RollbackAsync();
                    throw new BusinessException(
                        "Sản phẩm đã hết hàng hoặc số lượng còn lại không đủ. Vui lòng tải lại trang và thử lại.");
                }

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

                var shippingInfo = new ShippingInfo
                {
                    Order = newOrder,
                    Carrier = shippingCarrier,
                    TrackingNumber = trackingNumber,
                    Status = shippingStatus,
                    EstimatedArrival = estimatedArrival
                };

                _context.ShippingInfos.Add(shippingInfo);

                var reservation = new InventoryReservation
                {
                    Order = newOrder,
                    ProductId = productId,
                    Quantity = quantity,
                    Status = confirmInventoryImmediately
                        ? InventoryReservationStatus.Confirmed
                        : InventoryReservationStatus.Held,
                    CreatedAt = DateTime.UtcNow,
                    ConfirmedAt = confirmInventoryImmediately ? DateTime.UtcNow : null
                };

                _context.InventoryReservations.Add(reservation);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return newOrder;
            });
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

            foreach (var shipping in order.ShippingInfos)
            {
                shipping.Status = newShippingStatus;
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
                .Include(o => o.Payments)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o =>
                    o.Status == "Pending" &&
                    o.OrderDate < cutoffTime &&
                    o.Payments.Any(p => p.Status == "Pending" && p.Method == "PayPal"))
                .ToListAsync();
        }

        public async Task CancelOrderAsync(int orderId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var order = await _context.OrderTables
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    await transaction.RollbackAsync();
                    return;
                }

                if (order.Payments.Any(p => p.Status == "Paid"))
                {
                    await transaction.RollbackAsync();
                    return;
                }

                order.Status = "Cancelled";

                foreach (var payment in order.Payments.Where(p => p.Status == "Pending"))
                {
                    payment.Status = "Cancelled";
                }

                await ReleaseHeldReservationsAsync(order.Id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            });
        }

        public async Task<bool> FailPayPalPaymentAsync(
            int orderId,
            int buyerId,
            string paypalOrderId,
            string failureStatus)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var order = await _context.OrderTables
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o =>
                        o.Id == orderId &&
                        o.BuyerId == buyerId);

                var payment = order?.Payments
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefault(p =>
                        p.Method == "PayPal" &&
                        p.PayPalOrderId == paypalOrderId);

                if (order == null || payment == null || payment.Status == "Paid")
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                payment.Status = "Failed";
                order.Status = "Failed";

                await ReleaseHeldReservationsAsync(order.Id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            });
        }

        private async Task ReleaseHeldReservationsAsync(int orderId)
        {
            var releasedAt = DateTime.UtcNow;

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                DECLARE @Released TABLE
                (
                    [productId] int NOT NULL,
                    [quantity] int NOT NULL
                );

                UPDATE [InventoryReservation]
                SET [status] = {InventoryReservationStatus.Released},
                    [releasedAt] = {releasedAt}
                OUTPUT inserted.[productId], inserted.[quantity]
                    INTO @Released ([productId], [quantity])
                WHERE [orderId] = {orderId}
                  AND [status] = {InventoryReservationStatus.Held};

                UPDATE inventory
                SET inventory.[quantity] = ISNULL(inventory.[quantity], 0) + released.[quantity],
                    inventory.[lastUpdated] = {releasedAt}
                FROM [Inventory] AS inventory
                INNER JOIN
                (
                    SELECT [productId], SUM([quantity]) AS [quantity]
                    FROM @Released
                    GROUP BY [productId]
                ) AS released
                    ON released.[productId] = inventory.[productId];");
        }
    }
}
