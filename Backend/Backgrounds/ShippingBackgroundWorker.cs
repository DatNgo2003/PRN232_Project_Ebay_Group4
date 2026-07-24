using Backend.DTOs;
using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Backgrounds;

public class ShippingBackgroundWorker : BackgroundService
{
    private readonly IShippingTaskQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<OrderNotificationHub> _hubContext;
    private readonly ITransactionLogger _txLogger;
    private readonly ILogger<ShippingBackgroundWorker> _logger;

    public ShippingBackgroundWorker(
        IShippingTaskQueue queue,
        IServiceScopeFactory scopeFactory,
        IHubContext<OrderNotificationHub> hubContext,
        ITransactionLogger txLogger,
        ILogger<ShippingBackgroundWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _txLogger = txLogger;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ShippingBackgroundWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var taskMessage = await _queue.DequeueShippingTaskAsync(stoppingToken);
                await ProcessShippingTaskAsync(taskMessage, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing shipping task queue.");
            }
        }

        _logger.LogInformation("ShippingBackgroundWorker stopped.");
    }

    private async Task ProcessShippingTaskAsync(ShippingTaskMessage taskMessage, CancellationToken cancellationToken)
    {
        var txId = taskMessage.TransactionId ?? _txLogger.StartTransaction("ShippingWorker", "ProcessTask", taskMessage);
        _logger.LogInformation("Processing async shipping task for Order #{OrderId} | TxId={TxId}", taskMessage.OrderId, txId);

        // Safe Scope Resolution for Singleton HostedService -> Scoped DbContext & Services
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CloneEbayDbContext>();
        var shippingService = scope.ServiceProvider.GetRequiredService<IShippingService>();

        try
        {
            var address = await dbContext.Addresses.FirstOrDefaultAsync(a => a.Id == taskMessage.AddressId, cancellationToken);
            if (address == null)
            {
                address = new Address { City = "N/A", Country = "Vietnam" };
            }

            // Call shipping API (resilient client decorated with Polly / logging)
            var shipment = await shippingService.CreateShipmentAsync(
                address,
                taskMessage.EstimatedArrival,
                $"{taskMessage.UserId}-{taskMessage.ProductId}",
                cancellationToken);

            // Update database records for order shipping info
            var shippingInfo = await dbContext.ShippingInfos
                .FirstOrDefaultAsync(s => s.OrderId == taskMessage.OrderId, cancellationToken);

            if (shippingInfo != null)
            {
                shippingInfo.Carrier = shipment.Carrier;
                shippingInfo.TrackingNumber = shipment.TrackingNumber;
                shippingInfo.Status = shipment.Status;
                shippingInfo.EstimatedArrival = shipment.EstimatedArrival;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            _txLogger.LogSuccess(txId, "ShippingWorker", "CreateShipment", new
            {
                taskMessage.OrderId,
                shipment.TrackingNumber,
                shipment.Carrier,
                shipment.Status
            });

            // Push real-time SignalR notification to user and order channels
            var notificationData = new
            {
                OrderId = taskMessage.OrderId,
                TrackingNumber = shipment.TrackingNumber,
                Carrier = shipment.Carrier,
                Status = shipment.Status,
                EstimatedArrival = shipment.EstimatedArrival,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"User_{taskMessage.UserId}")
                .SendAsync("OrderShippingUpdated", notificationData, cancellationToken);

            await _hubContext.Clients.Group($"Order_{taskMessage.OrderId}")
                .SendAsync("OrderShippingUpdated", notificationData, cancellationToken);
        }
        catch (Exception ex)
        {
            _txLogger.LogFailure(txId, "ShippingWorker", "ProcessTask", ex, new { taskMessage.OrderId });
            _logger.LogError(ex, "Failed to process shipping task for Order #{OrderId}", taskMessage.OrderId);

            // Push failure notification via SignalR
            var failureData = new
            {
                OrderId = taskMessage.OrderId,
                Status = "Failed",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"User_{taskMessage.UserId}")
                .SendAsync("OrderShippingFailed", failureData, cancellationToken);

            await _hubContext.Clients.Group($"Order_{taskMessage.OrderId}")
                .SendAsync("OrderShippingFailed", failureData, cancellationToken);
        }
    }
}
