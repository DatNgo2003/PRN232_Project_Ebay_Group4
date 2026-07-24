using Backend.DTOs.Requests;
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
        private readonly IShippingFeeCalculator _shippingFeeCalculator;
        private readonly CloneEbayDbContext _context;
        private readonly IShippingService _shippingService;
        private readonly IPaymentGatewayFactory? _paymentGatewayFactory;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IEmailService emailService,
            ICouponRepository couponRepository,
            IShippingFeeCalculator shippingFeeCalculator,
            CloneEbayDbContext context,
            IShippingService? shippingService = null,
            IPaymentGatewayFactory? paymentGatewayFactory = null
        )
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _couponRepository = couponRepository;
            _shippingFeeCalculator = shippingFeeCalculator;
            _context = context;
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
            int? addressId,
            int quantity = 1,
            string? couponCode = null,
            string? carrierKey = null)
        {
            var user = await GetUserFromUsername(buyerUsername);
            if (user == null) return null;

            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null || product.Price == null) return null;

            if (quantity <= 0) quantity = 1;

            var address = addressId.HasValue
                ? await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId.Value && a.UserId == user.Id)
                : await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == user.Id && a.IsDefault == true);

            if (address == null)
                throw new BusinessException("Không tìm thấy địa chỉ giao hàng. Vui lòng chọn hoặc thêm địa chỉ.");

            var normalizedPaymentMethod = NormalizePaymentMethod(paymentMethod);
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

                await _couponRepository.IncrementUsedCountAsync(coupon.Id);
            }

            var totalAmount = Math.Max(0, subTotal - discountAmount + shippingFee);

            if (normalizedPaymentMethod == "COD" && _paymentGatewayFactory != null)
            {
                var codGateway = _paymentGatewayFactory.Resolve("COD");
                await codGateway.InitiateAsync(new PaymentInitiationRequest(
                    totalAmount,
                    "VND",
                    $"{user.Id}-{productId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    $"COD order - user {user.Id} - product {productId}"));
            }

            var paymentStatus = "Pending";
            var orderStatus = "Pending";
            var estimatedArrival = DateTime.UtcNow.AddDays(GetEstimatedDeliveryDays(address));
            var shipment = await _shippingService.CreateShipmentAsync(
                address,
                estimatedArrival,
                $"{user.Id}-{productId}",
                carrierKey);

            var order = await _orderRepository.CreateSimpleOrderAsync(
                user.Id,
                productId,
                product.Price.Value,
                shippingFee,
                normalizedPaymentMethod,
                paymentStatus,
                orderStatus,
                address.Id,
                shipment.TrackingNumber,
                estimatedArrival,
                quantity,
                subTotal,
                discountAmount,
                totalAmount,
                appliedCouponId);

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

        // >>> MỚI: checkout nhiều sản phẩm cùng lúc (giỏ hàng) — 1 Order, nhiều OrderItem
        public async Task<CartCheckoutResponseDto?> CreateCartOrderAsync(
            string buyerUsername,
            List<OrderItemRequestDto> items,
            string? paymentMethod,
            int? addressId,
            string? couponCode = null,
            string? carrierKey = null)
        {
            if (items == null || items.Count == 0)
                throw new BusinessException("Giỏ hàng đang trống.");

            var user = await GetUserFromUsername(buyerUsername);
            if (user == null) return null;

            var address = addressId.HasValue
                ? await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId.Value && a.UserId == user.Id)
                : await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == user.Id && a.IsDefault == true);

            if (address == null)
                throw new BusinessException("Không tìm thấy địa chỉ giao hàng. Vui lòng chọn hoặc thêm địa chỉ.");

            var normalizedPaymentMethod = NormalizePaymentMethod(paymentMethod);
            var shippingFee = _shippingFeeCalculator.Calculate(address);

            decimal subTotal = 0;
            var orderInputs = new List<CartOrderItemInput>();
            var responseItems = new List<CartCheckoutItemDto>();

            foreach (var item in items)
            {
                if (item.Quantity <= 0)
                    throw new BusinessException($"Số lượng sản phẩm {item.ProductId} không hợp lệ.");

                var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                if (product == null || product.Price == null)
                    throw new BusinessException($"Không tìm thấy sản phẩm id={item.ProductId} hoặc sản phẩm chưa có giá.");

                var lineTotal = Math.Round(product.Price.Value * item.Quantity, 2);
                subTotal += lineTotal;

                orderInputs.Add(new CartOrderItemInput(item.ProductId, item.Quantity, product.Price.Value));
                responseItems.Add(new CartCheckoutItemDto
                {
                    ProductId = item.ProductId,
                    ProductTitle = product.Title ?? "(Không có tên)",
                    Quantity = item.Quantity,
                    UnitPrice = product.Price.Value
                });
            }

            decimal discountAmount = 0;
            string? appliedCouponCode = null;
            int? appliedCouponId = null;

            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                var coupon = await _couponRepository.GetByCodeAsync(couponCode);
                ValidateCartCoupon(coupon, items);

                discountAmount = Math.Round(subTotal * (coupon!.DiscountPercent!.Value / 100m), 2);
                appliedCouponCode = coupon.Code;
                appliedCouponId = coupon.Id;

                await _couponRepository.IncrementUsedCountAsync(coupon.Id);
            }

            var totalAmount = Math.Max(0, subTotal - discountAmount + shippingFee);

            if (normalizedPaymentMethod == "COD" && _paymentGatewayFactory != null)
            {
                var codGateway = _paymentGatewayFactory.Resolve("COD");
                await codGateway.InitiateAsync(new PaymentInitiationRequest(
                    totalAmount,
                    "VND",
                    $"{user.Id}-cart-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    $"COD cart order - user {user.Id} - {items.Count} sản phẩm"));
            }

            var paymentStatus = "Pending";
            var orderStatus = "Pending";
            var estimatedArrival = DateTime.UtcNow.AddDays(GetEstimatedDeliveryDays(address));
            var shipment = await _shippingService.CreateShipmentAsync(
                address,
                estimatedArrival,
                $"{user.Id}-cart-{DateTime.UtcNow:yyyyMMddHHmmss}",
                carrierKey);

            var order = await _orderRepository.CreateMultiItemOrderAsync(
                user.Id,
                orderInputs,
                shippingFee,
                normalizedPaymentMethod,
                paymentStatus,
                orderStatus,
                address.Id,
                shipment.TrackingNumber,
                estimatedArrival,
                subTotal,
                discountAmount,
                totalAmount,
                appliedCouponId);

            return new CartCheckoutResponseDto
            {
                OrderId = order.Id,
                Items = responseItems,
                ShippingFee = shippingFee,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                PaymentMethod = normalizedPaymentMethod,
                PaymentStatus = paymentStatus,
                AddressId = address.Id,
                ShippingDestination = $"{address.City ?? "N/A"}, {address.Country ?? "N/A"}",
                TrackingNumber = shipment.TrackingNumber,
                ShippingStatus = shipment.Status,
                EstimatedArrival = estimatedArrival,
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

            var order = await _context.OrderTables
                .Include(o => o.Buyer)
                .Include(o => o.Payments)
                .Include(o => o.ShippingInfos)
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
                payment.Status = "Paid";
                payment.PaidAt = DateTime.UtcNow;
                payment.PayPalCaptureId = paypalCaptureId;
                order.Status = "Paid";
                await _context.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(order.Buyer?.Email))
                {
                    await _emailService.SendPaymentConfirmationEmailAsync(
                        toEmail: order.Buyer.Email,
                        buyerName: order.Buyer.Username ?? order.Buyer.Email,
                        orderId: order.Id,
                        totalAmount: order.TotalPrice ?? 0,
                        paymentMethod: "PayPal",
                        trackingNumber: order.ShippingInfos.FirstOrDefault()?.TrackingNumber,
                        productNames: order.OrderItems.Select(oi => oi.Product?.Title ?? "(Sản phẩm không xác định)"));
                }
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
        }

        public async Task CancelOrderAsync(int orderId)
        {
            await _orderRepository.CancelOrderAsync(orderId);
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

        // >>> MỚI: validate coupon cho giỏ hàng nhiều sản phẩm
        private void ValidateCartCoupon(Coupon? coupon, List<OrderItemRequestDto> items)
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
            if (coupon.DiscountPercent == null || coupon.DiscountPercent <= 0)
                throw new BusinessException("Mã giảm giá không hợp lệ");
            if (coupon.ProductId.HasValue && !items.Exists(i => i.ProductId == coupon.ProductId.Value))
                throw new BusinessException("Mã giảm giá không áp dụng cho sản phẩm nào trong giỏ hàng");
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
                    var shippingInfo = order.ShippingInfos
                        .OrderByDescending(s => s.EstimatedArrival)
                        .FirstOrDefault();

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
                        BuyerFeedbackRating = feedback?.AverageRating,

                        ShippingCarrier = shippingInfo?.Carrier,
                        TrackingNumber = shippingInfo?.TrackingNumber,
                        ShippingStatus = shippingInfo?.Status,
                        EstimatedArrival = shippingInfo?.EstimatedArrival,
                        ShippingFee = order.ShippingFee,

                        ShippingCity = order.Address?.City,
                        ShippingCountry = order.Address?.Country
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