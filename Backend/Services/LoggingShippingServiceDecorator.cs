using Backend.Models;
using Backend.Services.Shipping;

namespace Backend.Services;

/// <summary>
/// Decorator bọc quanh BẤT KỲ IShippingService nào để thêm log transaction + lỗi giao tiếp module.
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
        string? carrierKey = null,
        CancellationToken cancellationToken = default)
    {
        var txId = _txLogger.StartTransaction("Shipping", "CreateShipment",
            new { orderReference, destination.City, destination.Country, Carrier = carrierKey ?? "default" });

        try
        {
            var result = await _inner.CreateShipmentAsync(destination, estimatedArrival, orderReference, carrierKey, cancellationToken);
            _txLogger.LogSuccess(txId, "Shipping", "CreateShipment",
                new { result.TrackingNumber, result.Carrier, result.Status });
            return result;
        }
        catch (Exception ex)
        {
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
                    new InvalidOperationException("Carrier tu choi cap nhat (tracking number/status khong hop le)."),
                    new { trackingNumber, status });

            return ok;
        }
        catch (Exception ex)
        {
            _txLogger.LogInterModuleError(txId, "Order", "Shipping", "UpdateStatus", ex);
            throw;
        }
    }

    public IReadOnlyList<ShippingCarrierInfo> GetAvailableCarriers()
    {
        return _inner.GetAvailableCarriers();
    }

    public decimal EstimateFee(Address destination, decimal orderTotal, string carrierKey)
    {
        return _inner.EstimateFee(destination, orderTotal, carrierKey);
    }
}
