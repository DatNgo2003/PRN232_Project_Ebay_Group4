using Backend.Models;
using Backend.Services.Shipping;

namespace Backend.Services;

/// <summary>
/// Default shipping service that delegates to carrier-specific implementations
/// via <see cref="IShippingCarrierFactory"/>.
/// Falls back to the first registered carrier if none is specified.
/// </summary>
public sealed class DefaultShippingService : IShippingService
{
    private readonly IShippingCarrierFactory _carrierFactory;

    public DefaultShippingService(IShippingCarrierFactory carrierFactory)
    {
        _carrierFactory = carrierFactory;
    }

    public async Task<ShippingShipment> CreateShipmentAsync(
        Address destination,
        DateTime estimatedArrival,
        string orderReference,
        string? carrierKey = null,
        CancellationToken cancellationToken = default)
    {
        // Resolve carrier: use specified key, or fall back to first available
        var carriers = _carrierFactory.GetAvailableCarriers();
        var key = carrierKey;

        if (string.IsNullOrWhiteSpace(key) || !carriers.Any(c =>
            string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase)))
        {
            // Default to GHTK if available, otherwise first carrier
            key = carriers.FirstOrDefault(c => c.Key == "GHTK")?.Key
                  ?? carriers.First().Key;
        }

        var carrier = _carrierFactory.Resolve(key);
        var result = await carrier.CreateShipmentAsync(destination, orderTotal: 0, orderReference, cancellationToken);

        return new ShippingShipment(
            Carrier: carrier.DisplayName,
            TrackingNumber: result.TrackingNumber,
            Status: result.Status,
            EstimatedArrival: result.EstimatedArrival);
    }

    public async Task<bool> UpdateShipmentStatusAsync(
        string trackingNumber,
        string status,
        CancellationToken cancellationToken = default)
    {
        // Try each carrier until one accepts the tracking number
        foreach (var info in _carrierFactory.GetAvailableCarriers())
        {
            try
            {
                var carrier = _carrierFactory.Resolve(info.Key);
                var result = await carrier.UpdateStatusAsync(trackingNumber, status, cancellationToken);
                if (result) return true;
            }
            catch
            {
                // Continue to next carrier
            }
        }

        return false;
    }

    public IReadOnlyList<ShippingCarrierInfo> GetAvailableCarriers()
    {
        return _carrierFactory.GetAvailableCarriers();
    }

    public decimal EstimateFee(Address destination, decimal orderTotal, string carrierKey)
    {
        var carrier = _carrierFactory.Resolve(carrierKey);
        return carrier.EstimateFee(destination, orderTotal);
    }
}
