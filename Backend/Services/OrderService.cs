using Backend.DTOs.Responses;
using Backend.Models;
using Backend.Repositories;
using System;
using System.Linq;

namespace Backend.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserRepository userRepository
        )
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        private async Task<User?> GetUserFromUsername(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<QuickBuyCheckoutResponseDto?> CreateQuickBuyOrderAsync(
            string buyerUsername,
            int productId,
            string? paymentMethod,
            string? shippingRegion)
        {
            var user = await GetUserFromUsername(buyerUsername);
            if (user == null) return null;

            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null || product.Price == null) return null;

            var normalizedPaymentMethod = NormalizePaymentMethod(paymentMethod);
            var normalizedRegion = NormalizeShippingRegion(shippingRegion);
            var shippingFee = CalculateShippingFee(normalizedRegion);
            var paymentStatus = normalizedPaymentMethod == "PayPal" ? "Paid" : "Pending";
            // PayPal is simulated as paid, but payment and shipping are separate states.
            var orderStatus = "Pending";
            var estimatedArrival = DateTime.UtcNow.AddDays(GetEstimatedDeliveryDays(normalizedRegion));
            var trackingNumber = GenerateTrackingNumber(productId, user.Id);

            var order = await _orderRepository.CreateSimpleOrderAsync(
                user.Id,
                productId,
                product.Price.Value,
                shippingFee,
                normalizedPaymentMethod,
                paymentStatus,
                orderStatus,
                normalizedRegion,
                trackingNumber,
                estimatedArrival);

            return new QuickBuyCheckoutResponseDto
            {
                OrderId = order.Id,
                ProductPrice = product.Price.Value,
                ShippingFee = shippingFee,
                TotalAmount = product.Price.Value + shippingFee,
                PaymentMethod = normalizedPaymentMethod,
                PaymentStatus = paymentStatus,
                ShippingRegion = normalizedRegion,
                TrackingNumber = trackingNumber,
                EstimatedArrival = estimatedArrival
            };
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

        private static string NormalizeShippingRegion(string? shippingRegion)
        {
            var region = shippingRegion?.Trim().ToLowerInvariant();

            return region switch
            {
                "north" or "northern" or "ha noi" or "hanoi" => "North",
                "central" or "middle" or "da nang" or "danang" => "Central",
                "south" or "southern" or "ho chi minh" or "hcm" or "sai gon" or "saigon" => "South",
                "international" or "overseas" => "International",
                _ => "South"
            };
        }

        private static decimal CalculateShippingFee(string shippingRegion)
        {
            return shippingRegion switch
            {
                "North" => 5.00m,
                "Central" => 7.50m,
                "South" => 6.00m,
                "International" => 20.00m,
                _ => 10.00m
            };
        }

        private static int GetEstimatedDeliveryDays(string shippingRegion)
        {
            return shippingRegion switch
            {
                "North" => 3,
                "Central" => 4,
                "South" => 2,
                "International" => 10,
                _ => 5
            };
        }

        private static string GenerateTrackingNumber(int productId, int buyerId)
        {
            return $"MOCK-{DateTime.UtcNow:yyyyMMddHHmmss}-{buyerId}-{productId}-{Guid.NewGuid():N}"[..40];
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
                    ProductId = item.Product.Id, // Đã có sẵn
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

                    // SỬA ĐỔI: Gán tên người bán thật
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
    }
}
