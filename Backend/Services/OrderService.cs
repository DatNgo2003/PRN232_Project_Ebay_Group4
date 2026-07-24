using Backend.DTOs.Responses;
using Backend.Exceptions;
using Backend.Models;
using Backend.Repositories;
using Backend.Services.PaymentGateways;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Backend.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ICouponRepository _couponRepository;
        private readonly IShippingFeeCalculator _shippingFeeCalculator; // >>> THÊM: dùng chung chức năng 2 
        private readonly CloneEbayDbContext _context;                   // >>> THÊM: để lookup Address 
        private readonly IShippingService _shippingService;
        private readonly IPaymentGatewayFactory? _paymentGatewayFactory; // >>> MỚI: plug-in cổng thanh toán 

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IEmailService emailService,
            ICouponRepository couponRepository,
            IShippingFeeCalculator shippingFeeCalculator, // >>> THÊM 
            CloneEbayDbContext context,                    // >>> THÊM 
            IShippingService? shippingService = null,
            IPaymentGatewayFactory? paymentGatewayFactory = null // >>> MỚI 
        )
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _couponRepository = couponRepository;
            _shippingFeeCalculator = shippingFeeCalculator;
            _context = context;
            // Optional fallback keeps the service usable by existing callers
            // while production DI supplies the singleton provider.
            _shippingService = shippingService ?? new MockShippingService();
            _paymentGatewayFactory = paymentGatewayFactory;
        }

        private async Task<User?> GetUserFromUsername(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<QuickBuyCheckoutResponseDto?> CreateQuickBuyOrderAsync(
            string buyerUsername,
            int productId,
            string? paymentMethod,
            int? addressId,            // >>> SỬA: thay shippingRegion 
            int quantity = 1,
            string? couponCode = null)
        {
            var user = await GetUserFromUsername(buyerUsername);
            if (user == null) return null;

            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null || product.Price == null) return null;

            if (quantity <= 0) quantity = 1;

            // >>> SỬA: lấy Address thật thay vì tự map region string 
            var address = addressId.HasValue
                ? await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId.Value && a.UserId == user.Id)
                : await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == user.Id && a.IsDefault == true);

            if (address == null)
                throw new BusinessException("Không tìm thấy địa chỉ giao hàng. Vui lòng chọn hoặc thêm địa chỉ.");

            var normalizedPaymentMethod = NormalizePaymentMethod(paymentMethod);

            // >>> SỬA: dùng đúng IShippingFeeCalculator (chức năng 2) thay vì bảng giá tự chế 
            var shippingFee = _shippingFeeCalculator.Calculate(address);

            var subTotal = Math.Round(product.Price.Value * quantity, 2);
            decimal discountAmount = 0;
            string? appliedCouponCode = null;
            int? appliedCouponId = null;

            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                var coupon = await _couponRepository.GetByCodeAsync(couponCode);
                ValidateCoupon(coupon, productId);

                discountAmount = Math.Round(subTotal * (coupon!.DiscountPercent!.Value / 100m), 2);
                appliedCouponCode = coupon.Code;
                appliedCouponId = coupon.Id;

            }

            var totalAmount = Math.Max(0, subTotal - discountAmount + shippingFee);

            // >>> MỚI: gọi cổng thanh toán qua kiến trúc plug-in (IPaymentGatewayFactory).
            // Với COD, bước "khởi tạo" đồng thời log lại transaction để truy vết.
            // PayPal được khởi tạo ở PayPalController (sau khi có OrderId làm reference_id).
            if (normalizedPaymentMethod == "COD" && _paymentGatewayFactory != null)
            {
                var codGateway = _paymentGatewayFactory.Resolve("COD");
                await codGateway.InitiateAsync(new PaymentInitiationRequest(
                    totalAmount,
                    "VND",
                    $"{user.Id}-{productId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    $"COD order - user {user.Id} - product {productId}"));
            }

            // PayPal is not considered paid until the server receives a successful
            // capture response from PayPal. The PayPal checkout flow creates this
            // local order as Pending and completes it in CompletePayPalPaymentAsync.
            var paymentStatus = "Pending";
            var orderStatus = "Pending";
            var estimatedArrival = DateTime.UtcNow.AddDays(GetEstimatedDeliveryDays(address));
            var shipment = await _shippingService.CreateShipmentAsync(
                address,
                estimatedArrival,
                $"{user.Id}-{productId}");

            var order = await _orderRepository.CreateSimpleOrderAsync(
                user.Id,
                productId,
                product.Price.Value,
                shippingFee,
                normalizedPaymentMethod,
                paymentStatus,
                orderStatus,
                address.Id,          // >>> SỬA: truyền addressId thay vì region string
                shipment.Carrier,
                shipment.Status,
                shipment.TrackingNumber,
                estimatedArrival,
                quantity,
                subTotal,
                discountAmount,
                totalAmount,
                appliedCouponId,
                confirmInventoryImmediately: normalizedPaymentMethod == "COD");

            if (appliedCouponId.HasValue)
            {
                await _couponRepository.IncrementUsedCountAsync(appliedCouponId.Value);
            }

            return new QuickBuyCheckoutResponseDto
            {
                OrderId = order.Id,
                ProductPrice = product.Price.Value,
                ShippingFee = shippingFee,
                TotalAmount = totalAmount,
                PaymentMethod = normalizedPaymentMethod,
                PaymentStatus = paymentStatus,

                AddressId = address.Id,
                ShippingDestination = $"{address.City ?? "N/A"}, {address.Country ?? "N/A"}",

                TrackingNumber = shipment.TrackingNumber,
                ShippingStatus = shipment.Status,
                EstimatedArrival = estimatedArrival,

                Quantity = quantity,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                AppliedCoupon = appliedCouponCode
            };
        }

        public async Task<bool> AttachPayPalOrderAsync(
            string buyerUsername,
            int orderId,
            string paypalOrderId)
        {
            if (string.IsNullOrWhiteSpace(paypalOrderId)) return false;

            var user = await GetUserFromUsername(buyerUsername);
            if (user == null) return false;

            var order = await _context.OrderTables
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerId == user.Id);

            var payment = order?.Payments
                .OrderByDescending(p => p.Id)
                .FirstOrDefault(p => p.Method == "PayPal");

            if (payment == null || payment.Status != "Pending") return false;

            payment.PayPalOrderId = paypalOrderId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsPayPalOrderOwnedAsync(
            string buyerUsername,
            int orderId,
            string paypalOrderId)
        {
            if (string.IsNullOrWhiteSpace(paypalOrderId)) return false;

            var user = await GetUserFromUsername(buyerUsername);
            if (user == null) return false;

            return await _context.Payments.AnyAsync(p =>
                p.OrderId == orderId &&
                p.UserId == user.Id &&
                p.Method == "PayPal" &&
                p.PayPalOrderId == paypalOrderId &&
                (p.Status == "Pending" || p.Status == "Paid"));
        }

        public async Task<PayPalPaymentCompletionResultDto?> CompletePayPalPaymentAsync(
            string buyerUsername,
            int orderId,
            string paypalOrderId,
            string paypalCaptureId,
            decimal capturedAmount,
            string currency)
        {
            var user = await GetUserFromUsername(buyerUsername);
            if (user == null) return null;

            var strategy = _context.Database.CreateExecutionStrategy();
            PaymentConfirmationNotification? notification = null;

            var result = await strategy.ExecuteAsync<PayPalPaymentCompletionResultDto?>(async () =>
            {
                notification = null;
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var order = await _context.OrderTables
                    .Include(o => o.Buyer)
                    .Include(o => o.Payments)
                    .Include(o => o.ShippingInfos)
                    .Include(o => o.InventoryReservations)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerId == user.Id);

                var payment = order?.Payments
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefault(p => p.Method == "PayPal" && p.PayPalOrderId == paypalOrderId);

                if (order == null || payment == null) return null;

                if (order.TotalPrice == null ||
                    order.TotalPrice.Value != capturedAmount ||
                    !string.Equals(currency, "USD", StringComparison.OrdinalIgnoreCase))
                {
                    throw new BusinessException("Số tiền hoặc loại tiền PayPal không khớp với đơn hàng.");
                }

                if (payment.Status != "Paid")
                {
                    if (order.InventoryReservations.Any(r =>
                        r.Status == InventoryReservationStatus.Released))
                    {
                        throw new BusinessException(
                            "Giữ hàng cho đơn này đã hết hạn. Không thể hoàn tất thanh toán.");
                    }

                    payment.Status = "Paid";
                    payment.PaidAt = DateTime.UtcNow;
                    payment.PayPalCaptureId = paypalCaptureId;
                    order.Status = "Paid";

                    foreach (var reservation in order.InventoryReservations.Where(r =>
                        r.Status == InventoryReservationStatus.Held))
                    {
                        reservation.Status = InventoryReservationStatus.Confirmed;
                        reservation.ConfirmedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    if (!string.IsNullOrWhiteSpace(order.Buyer?.Email))
                    {
                        notification = new PaymentConfirmationNotification(
                            order.Buyer.Email,
                            order.Buyer.Username ?? order.Buyer.Email,
                            order.Id,
                            order.TotalPrice ?? 0,
                            order.ShippingInfos.FirstOrDefault()?.TrackingNumber,
                            order.OrderItems
                                .Select(oi => oi.Product?.Title ?? "(Sản phẩm không xác định)")
                                .ToArray());
                    }
                }
                else
                {
                    await transaction.RollbackAsync();
                }

                return new PayPalPaymentCompletionResultDto
                {
                    OrderId = order.Id,
                    PayPalOrderId = paypalOrderId,
                    PayPalCaptureId = paypalCaptureId,
                    PaymentStatus = payment.Status ?? "Paid",
                    TotalAmount = order.TotalPrice ?? 0,
                    Currency = currency
                };
            });

            if (notification != null)
            {
                await _emailService.SendPaymentConfirmationEmailAsync(
                    toEmail: notification.ToEmail,
                    buyerName: notification.BuyerName,
                    orderId: notification.OrderId,
                    totalAmount: notification.TotalAmount,
                    paymentMethod: "PayPal",
                    trackingNumber: notification.TrackingNumber,
                    productNames: notification.ProductNames);
            }

            return result;
        }

        public async Task CancelOrderAsync(int orderId)
        {
            await _orderRepository.CancelOrderAsync(orderId);
        }

        public async Task<bool> FailPayPalPaymentAsync(
            string buyerUsername,
            int orderId,
            string paypalOrderId,
            string failureStatus)
        {
            var user = await GetUserFromUsername(buyerUsername);
            if (user == null) return false;

            return await _orderRepository.FailPayPalPaymentAsync(
                orderId,
                user.Id,
                paypalOrderId,
                failureStatus);
        }

        private void ValidateCoupon(Coupon? coupon, int productId)
        {
            if (coupon == null)
                throw new BusinessException("Mã giảm giá không tồn tại");

            var now = DateTime.UtcNow;
            if (coupon.StartDate.HasValue && coupon.StartDate > now)
                throw new BusinessException("Mã giảm giá chưa có hiệu lực");
            if (coupon.EndDate.HasValue && coupon.EndDate < now)
                throw new BusinessException("Mã giảm giá đã hết hạn");
            if (coupon.MaxUsage.HasValue && (coupon.UsedCount ?? 0) >= coupon.MaxUsage.Value)
                throw new BusinessException("Mã giảm giá đã hết lượt sử dụng");
            if (coupon.ProductId.HasValue && coupon.ProductId.Value != productId)
                throw new BusinessException("Mã giảm giá không áp dụng cho sản phẩm này");
            if (coupon.DiscountPercent == null || coupon.DiscountPercent <= 0)
                throw new BusinessException("Mã giảm giá không hợp lệ");
        }

        private static string NormalizePaymentMethod(string? paymentMethod)
        {
            return paymentMethod?.Trim().ToUpperInvariant() switch
            {
                "PAYPAL" => "PayPal",
                "COD" => "COD",
                _ => "COD"
            };
        }

        // >>> SỬA: bỏ NormalizeShippingRegion + CalculateShippingFee(string) — không còn dùng,
        // phí ship giờ lấy từ IShippingFeeCalculator (chức năng 2) 

        // >>> SỬA: ước tính ngày giao hàng theo Address thay vì region string,
        // dùng cùng tiêu chí phân loại nội thành/tỉnh khác/quốc tế như SimpleRegionShippingFeeCalculator 

        private static int GetEstimatedDeliveryDays(Address address)
        {
            if (address.Country != null &&
                !address.Country.Equals("Vietnam", StringComparison.OrdinalIgnoreCase) &&
                !address.Country.Equals("Việt Nam", StringComparison.OrdinalIgnoreCase))
            {
                return 10;
            }

            if (Backend.Services.Implementation.RegionHelper.IsInnerCity(address.City))
            {
                return 2;
            }

            return 5;
        }

        private sealed record PaymentConfirmationNotification(
            string ToEmail,
            string BuyerName,
            int OrderId,
            decimal TotalAmount,
            string? TrackingNumber,
            IReadOnlyCollection<string> ProductNames);

        public async Task<IEnumerable<PurchaseHistoryItemDto>> GetPurchaseHistoryAsync(string buyerUsername)
        {
            var user = await GetUserFromUsername(buyerUsername);
            if (user == null)
            {
                return new List<PurchaseHistoryItemDto>();
            }

            var orderItems = await _orderRepository.GetPurchaseHistoryAsync(user.Id);

            var dtos = new List<PurchaseHistoryItemDto>();
            var now = DateTime.UtcNow;

            foreach (var item in orderItems)
            {
                if (item.Product == null || item.Order == null) continue;

                var order = item.Order;
                var payment = order.Payments.OrderByDescending(p => p.PaidAt ?? DateTime.MinValue).FirstOrDefault();
                var shippingInfo = order.ShippingInfos.OrderByDescending(s => s.EstimatedArrival ?? DateTime.MinValue).FirstOrDefault();
                string feedbackState;

                if (order.Feedbacks.Any())
                {
                    feedbackState = "SUBMITTED";
                }
                else if (order.Disputes.Any(d => d.Status == "Pending") ||
                         order.ReturnRequests.Any(r => r.Status == "Pending"))
                {
                    feedbackState = "IN_DISPUTE";
                }
                else if (order.Status != "Completed")
                {
                    feedbackState = "PENDING_DELIVERY";
                }
                else if ((now - (order.OrderDate ?? now.AddDays(-100))).TotalDays > 60)
                {
                    feedbackState = "EXPIRED";
                }
                else
                {
                    feedbackState = "ELIGIBLE";
                }

                dtos.Add(new PurchaseHistoryItemDto
                {
                    OrderItemId = item.Id,
                    OrderId = item.OrderId ?? 0,
                    ProductId = item.Product.Id,
                    ProductTitle = item.Product.Title,
                    ProductImage = item.Product.Images,
                    UnitPrice = item.UnitPrice,
                    OrderTotalPrice = order.TotalPrice,
                    OrderDate = item.Order.OrderDate ?? DateTime.MinValue,

                    FeedbackState = feedbackState,
                    OrderStatus = order.Status,
                    PaymentMethod = payment?.Method,
                    PaymentStatus = payment?.Status,
                    ShippingCarrier = shippingInfo?.Carrier,
                    ShippingStatus = shippingInfo?.Status,
                    TrackingNumber = shippingInfo?.TrackingNumber,
                    EstimatedArrival = shippingInfo?.EstimatedArrival,

                    SellerUsername = item.Product.Seller?.Username ?? "Unknown Seller",
                    SellerId = item.Product.Seller?.Id ?? 0
                });
            }

            return dtos;
        }

        public async Task<IEnumerable<SellerSalesOrderDto>> GetSalesHistoryAsync(string sellerUsername)
        {
            var user = await GetUserFromUsername(sellerUsername);
            if (user == null || (user.Role != "seller" && user.Role != "supporter"))
            {
                return new List<SellerSalesOrderDto>();
            }

            var orderItems = await _orderRepository.GetOrderItemsBySellerIdAsync(user.Id);

            var groupedOrders = orderItems
                .GroupBy(oi => oi.Order)
                .Select(group =>
                {
                    var order = group.Key;
                    var feedback = order.Feedbacks.FirstOrDefault();

                    return new SellerSalesOrderDto
                    {
                        OrderId = order.Id,
                        OrderDate = order.OrderDate,
                        OrderStatus = order.Status,
                        OrderTotalPrice = order.TotalPrice,

                        BuyerId = order.BuyerId ?? 0,
                        BuyerUsername = order.Buyer?.Username ?? "Unknown Buyer",

                        Items = group.Select(oi => new SellerSalesItemDto
                        {
                            ProductId = oi.ProductId ?? 0,
                            ProductTitle = oi.Product?.Title,
                            Quantity = oi.Quantity ?? 0,
                            UnitPrice = oi.UnitPrice
                        }).ToList(),

                        HasBuyerFeedback = feedback != null,
                        BuyerFeedbackId = feedback?.Id,
                        BuyerFeedbackRating = feedback?.AverageRating
                    };
                });

            return groupedOrders.ToList();
        }

        public async Task<bool> UpdateShippingStatusAsync(int orderId, string newShippingStatus)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order == null) return false;

            var trackingNumber = order.ShippingInfos
                .OrderByDescending(s => s.EstimatedArrival)
                .FirstOrDefault()?.TrackingNumber;
            if (string.IsNullOrWhiteSpace(trackingNumber)) return false;

            // Update the simulated carrier first. Do not change our order if
            // the carrier rejects the tracking number/status.
            var carrierUpdated = await _shippingService.UpdateShipmentStatusAsync(
                trackingNumber,
                newShippingStatus);
            if (!carrierUpdated) return false;

            await _orderRepository.UpdateShippingStatusAsync(orderId, newShippingStatus);

            bool shouldNotify =
                newShippingStatus.Equals("Delivered", StringComparison.OrdinalIgnoreCase) ||
                newShippingStatus.Equals("Failed", StringComparison.OrdinalIgnoreCase);

            if (shouldNotify && order.Buyer?.Email != null)
            {
                var productNames = order.OrderItems
                    .Select(oi => oi.Product?.Title ?? "(Sản phẩm không xác định)")
                    .ToList();

                await _emailService.SendShippingStatusEmailAsync(
                    toEmail: order.Buyer.Email,
                    buyerName: order.Buyer.Username ?? order.Buyer.Email,
                    orderId: order.Id,
                    shippingStatus: newShippingStatus,
                    productNames: productNames,
                    trackingNumber: trackingNumber);
            }

            return true;
        }
    }
}
