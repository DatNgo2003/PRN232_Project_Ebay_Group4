using Backend.Models;
using Backend.Services.Shipping;
using System.Collections.Concurrent;

namespace Backend.Services;

/// <summary>
/// In-process shipping API simulator. It creates carrier-like tracking numbers
/// and keeps the latest status so callers exercise the same create/update flow
/// as they would with an external provider.
/// </summary>
public sealed class MockShippingService : IShippingService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Processing", "Shipped", "InTransit", "OutForDelivery", "Delivered", "Failed"
    };

    private readonly ConcurrentDictionary<string, string> _statuses = new(StringComparer.OrdinalIgnoreCase);

    public Task<ShippingShipment> CreateShipmentAsync(
        Address destination,
        DateTime estimatedArrival,
        string orderReference,
        string? carrierKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(destination);
        cancellationToken.ThrowIfCancellationRequested();

        // MOCK- is intentional: it makes test/demo shipments distinguishable
        // from production carrier numbers while remaining safe for DB's 100-char column.
        var safeReference = string.IsNullOrWhiteSpace(orderReference) ? "ORDER" : orderReference.Trim();
        // Keep the value within ShippingInfo.trackingNumber (nvarchar(100)).
        safeReference = safeReference.Length > 20 ? safeReference[..20] : safeReference;
        var trackingNumber = $"MOCK-{safeReference}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
        _statuses[trackingNumber] = "Processing";

        var carrier = string.IsNullOrWhiteSpace(carrierKey)
            ? $"MockExpress - {destination.City ?? destination.Country ?? "N/A"}"
            : carrierKey;
        return Task.FromResult(new ShippingShipment(carrier, trackingNumber, "Processing", estimatedArrival));
    }

    public Task<bool> UpdateShipmentStatusAsync(
        string trackingNumber,
        string status,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedTrackingNumber = trackingNumber?.Trim();
        var normalizedStatus = status?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTrackingNumber) ||
            string.IsNullOrWhiteSpace(normalizedStatus) ||
            !AllowedStatuses.Contains(normalizedStatus))
            return Task.FromResult(false);

        // Existing orders may predate this provider instance. Accepting a
        // non-empty tracking number mirrors a carrier lookup/update call and
        // lets those orders transition as well.
        _statuses.AddOrUpdate(normalizedTrackingNumber, normalizedStatus, (_, _) => normalizedStatus);
        return Task.FromResult(true);
    }

    public IReadOnlyList<ShippingCarrierInfo> GetAvailableCarriers()
    {
        return new List<ShippingCarrierInfo>
        {
            new("MOCK", "Mock Express", "Mock carrier for testing")
        }.AsReadOnly();
    }

    public decimal EstimateFee(Address destination, decimal orderTotal, string carrierKey)
    {
        // Mock: flat $5.00 shipping fee
        return 5.00m;
    }
}
