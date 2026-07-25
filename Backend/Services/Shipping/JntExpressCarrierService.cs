using System.Collections.Concurrent;
using Backend.Models;

namespace Backend.Services.Shipping;

/// <summary>
/// Simulates J&T Express Vietnam shipping API.
/// Tracking format: JT-XXXXXXXXXXXXXXXX (16 hex chars).
/// Fee: 20k VND base domestic, 35k inter-regional, 150k international.
/// Free ship for orders >= 500k within same city.
/// Estimated delivery: 2-3 days domestic, 4-6 days inter-regional.
/// </summary>
public sealed class JntExpressCarrierService : IShippingCarrierService
{
    private static readonly ConcurrentDictionary<string, string> _statuses = new(StringComparer.OrdinalIgnoreCase);

    public string CarrierKey => "JT_EXPRESS";
    public string DisplayName => "J&T Express";

    public Task<CarrierShipmentResult> CreateShipmentAsync(
        Address destination,
        decimal orderTotal,
        string orderReference,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // J&T tracking: 16-char hex
        var trackingNumber = $"JT-{Guid.NewGuid().ToString("N")[..16].ToUpper()}";
        _statuses[trackingNumber] = "Processing";

        var estimatedDays = GetEstimatedDays(destination);
        var estimatedArrival = DateTime.UtcNow.AddDays(estimatedDays);

        return Task.FromResult(new CarrierShipmentResult(
            trackingNumber,
            "Processing",
            estimatedArrival));
    }

    public Task<bool> UpdateStatusAsync(
        string trackingNumber,
        string status,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(trackingNumber) || string.IsNullOrWhiteSpace(status))
            return Task.FromResult(false);

        var allowed = new[] { "Processing", "Shipped", "InTransit", "OutForDelivery", "Delivered", "Failed" };
        if (!allowed.Contains(status, StringComparer.OrdinalIgnoreCase))
            return Task.FromResult(false);

        _statuses.AddOrUpdate(trackingNumber.Trim(), status, (_, _) => status);
        return Task.FromResult(true);
    }

    public decimal EstimateFee(Address destination, decimal orderTotal)
    {
        // J&T: free ship đơn >= 500k trong cùng thành phố
        if (orderTotal >= 500_000m && IsHanoiOrHcm(destination.City))
            return 0m;

        if (IsHanoiOrHcm(destination.City))
            return 20_000m;

        if (IsVietnam(destination.Country))
            return 35_000m;

        return 150_000m; // Quốc tế
    }

    private static int GetEstimatedDays(Address destination)
    {
        if (IsHanoiOrHcm(destination.City)) return 2;
        if (IsVietnam(destination.Country)) return 4;
        return 7;
    }

    private static bool IsHanoiOrHcm(string? city)
    {
        if (string.IsNullOrWhiteSpace(city)) return false;
        return city.Contains("Ha Noi", StringComparison.OrdinalIgnoreCase) ||
               city.Contains("Hà Nội", StringComparison.OrdinalIgnoreCase) ||
               city.Contains("Ho Chi Minh", StringComparison.OrdinalIgnoreCase) ||
               city.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) ||
               city.Contains("TP.HCM", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsVietnam(string? country)
    {
        if (string.IsNullOrWhiteSpace(country)) return false;
        return country.Equals("Vietnam", StringComparison.OrdinalIgnoreCase) ||
               country.Equals("Việt Nam", StringComparison.OrdinalIgnoreCase);
    }
}
