using Backend.Repositories;
using Backend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.Backgrounds
{
    /// <summary>
    /// Background service chạy định kỳ để tự động huỷ các đơn hàng
    /// quá thời gian chờ thanh toán (cấu hình tại OrderSettings:PaymentTimeoutMinutes).
    /// Mặc định: 30 phút. Kiểm tra mỗi 5 phút.
    /// </summary>
    public class OrderCancellationService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<OrderCancellationService> _logger;
        private readonly IConfiguration _configuration;

        // Kiểm tra mỗi 5 phút
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

        public OrderCancellationService(
            IServiceProvider services,
            ILogger<OrderCancellationService> logger,
            IConfiguration configuration)
        {
            _services = services;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderCancellationService đã khởi động.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredOrdersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong OrderCancellationService khi xử lý đơn hàng quá hạn.");
                }

                // Chờ 5 phút trước lần kiểm tra tiếp theo
                await Task.Delay(CheckInterval, stoppingToken);
            }

            _logger.LogInformation("OrderCancellationService đã dừng.");
        }

        private async Task ProcessExpiredOrdersAsync()
        {
            // Lấy cấu hình timeout (phút)
            var timeoutMinutes = _configuration.GetValue<int>("OrderSettings:PaymentTimeoutMinutes", 30);
            var cutoffTime = DateTime.UtcNow.AddMinutes(-timeoutMinutes);

            using var scope = _services.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Tìm các đơn hàng Pending, chưa thanh toán, quá hạn
            var expiredOrders = await orderRepository.GetPendingPaymentOrdersAsync(cutoffTime);

            if (!expiredOrders.Any())
            {
                _logger.LogDebug("Không có đơn hàng nào quá hạn thanh toán tại {Time}", DateTimeOffset.Now);
                return;
            }

            _logger.LogInformation(
                "Tìm thấy {Count} đơn hàng quá hạn thanh toán (cutoff: {CutoffTime}). Bắt đầu huỷ...",
                expiredOrders.Count(), cutoffTime);

            foreach (var order in expiredOrders)
            {
                try
                {
                    // Huỷ đơn hàng trong DB
                    await orderRepository.CancelOrderAsync(order.Id);

                    _logger.LogInformation(
                        "Đã huỷ đơn hàng #{OrderId} (buyer: {BuyerEmail}) do quá {Timeout} phút chưa thanh toán.",
                        order.Id,
                        order.Buyer?.Email ?? "N/A",
                        timeoutMinutes);

                    // Gửi email thông báo nếu buyer có email
                    if (order.Buyer?.Email != null)
                    {
                        var productNames = order.OrderItems
                            .Select(oi => oi.Product?.Title ?? "(Sản phẩm không xác định)")
                            .ToList();

                        await emailService.SendOrderCancelledEmailAsync(
                            toEmail: order.Buyer.Email,
                            buyerName: order.Buyer.Username ?? order.Buyer.Email,
                            orderId: order.Id,
                            productNames: productNames,
                            totalAmount: order.TotalPrice ?? 0);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi huỷ đơn hàng #{OrderId}", order.Id);
                }
            }
        }
    }
}
