using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Decorator (plug-in) bọc quanh BẤT KỲ IShippingService nào (Mock hiện tại,
/// hoặc carrier thật sau này) để thêm log transaction + lỗi giao tiếp module,
/// mà không cần sửa MockShippingService hay implementation thật.
/// </summary>
public sealed class LoggingShippingServiceDecorator : IShippingService
{
    private readonly IShippingService _inner;
    private readonly ITransactionLogger _txLogger;

    public LoggingShippingServiceDecorator(IShippingService inner, ITransactionLogger txLogger)
    {
        _inner = inner;
        _txLogger = txLogger;
    }

    public async Task<ShippingShipment> CreateShipmentAsync(
        Address destination,
        DateTime estimatedArrival,
        string orderReference,
        CancellationToken cancellationToken = default)
    {
        var txId = _txLogger.StartTransaction("Shipping", "CreateShipment",
            new { orderReference, destination.City, destination.Country });

        try
        {
            var result = await _inner.CreateShipmentAsync(destination, estimatedArrival, orderReference, cancellationToken);
            _txLogger.LogSuccess(txId, "Shipping", "CreateShipment",
                new { result.TrackingNumber, result.Carrier, result.Status });
            return result;
        }
        catch (Exception ex)
        {
            // Lỗi giao tiếp giữa module Order và module Shipping (carrier API)
            _txLogger.LogInterModuleError(txId, "Order", "Shipping", "CreateShipment", ex);
            throw;
        }
    }

    public async Task<bool> UpdateShipmentStatusAsync(
        string trackingNumber,
        string status,
        CancellationToken cancellationToken = default)
    {
        var txId = _txLogger.StartTransaction("Shipping", "UpdateStatus", new { trackingNumber, status });

        try
        {
            var ok = await _inner.UpdateShipmentStatusAsync(trackingNumber, status, cancellationToken);

            if (ok)
                _txLogger.LogSuccess(txId, "Shipping", "UpdateStatus", new { trackingNumber, status });
            else
                _txLogger.LogFailure(txId, "Shipping", "UpdateStatus",
                    new InvalidOperationException("Carrier từ chối cập nhật (tracking number/status không hợp lệ)."),
                    new { trackingNumber, status });

            return ok;
        }
        catch (Exception ex)
        {
            _txLogger.LogInterModuleError(txId, "Order", "Shipping", "UpdateStatus", ex);
            throw;
        }
    }
}