using Backend.Models;
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
        "Preparing", "Shipping", "Delivered", "Failed"
    };

    private readonly ConcurrentDictionary<string, string> _statuses = new(StringComparer.OrdinalIgnoreCase);
    private readonly HttpClient? _httpClient;

    public MockShippingService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient;
    }

    public async Task<ShippingShipment> CreateShipmentAsync(
        Address destination,
        DateTime estimatedArrival,
        string orderReference,
        CancellationToken cancellationToken = default)
    {
        throw new HttpRequestException("Lỗi mạng giả lập: Không thể kết nối tới server vận chuyển!");
        ArgumentNullException.ThrowIfNull(destination);
        cancellationToken.ThrowIfCancellationRequested();

        // --- TEMPORARY FAILURE CODE FOR TESTING POLLY ---
        if (_httpClient != null)
        {
            // This will fail (Connection Refused) and trigger Polly's exponential backoff
            await _httpClient.GetAsync("http://localhost:9999/trigger-failure", cancellationToken);
        }
        // ------------------------------------------------

        // MOCK- is intentional: it makes test/demo shipments distinguishable
        // from production carrier numbers while remaining safe for DB's 100-char column.
        var safeReference = string.IsNullOrWhiteSpace(orderReference) ? "ORDER" : orderReference.Trim();
        // Keep the value within ShippingInfo.trackingNumber (nvarchar(100)).
        safeReference = safeReference.Length > 20 ? safeReference[..20] : safeReference;
        var trackingNumber = $"MOCK-{safeReference}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
        _statuses[trackingNumber] = "Preparing";

        var carrier = $"MockExpress - {destination.City ?? destination.Country ?? "N/A"}";
        return new ShippingShipment(carrier, trackingNumber, "Preparing", estimatedArrival);
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
}
