using System.Collections.Concurrent;
using Backend.Models;

namespace Backend.Services.Shipping;

/// <summary>
/// Simulates Giao Hàng Tiết Kiệm (GHTK) shipping API.
/// Tracking format: GHTK-XXXXXXXX (8 hex chars).
/// Fee: 15k VND base + 2k/km domestic, free ship under 150k orders in HCM/HN.
/// Estimated delivery: 2-4 days domestic, 5-7 days inter-regional.
/// </summary>
public sealed class GhtkCarrierService : IShippingCarrierService
{
    private static readonly ConcurrentDictionary<string, string> _statuses = new(StringComparer.OrdinalIgnoreCase);

    public string CarrierKey => "GHTK";
    public string DisplayName => "Giao Hàng Tiết Kiệm (GHTK)";

    public Task<CarrierShipmentResult> CreateShipmentAsync(
        Address destination,
        decimal orderTotal,
        string orderReference,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var trackingNumber = $"GHTK-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
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
        // GHTK: free ship cho đơn < 150k trong HN/HCM
        if (orderTotal < 150_000m &&
            IsHanoiOrHcm(destination.City))
        {
            return 0m;
        }

        // Phí cơ bản 15k, cộng thêm theo khu vực
        if (IsHanoiOrHcm(destination.City))
            return 15_000m;

        if (IsVietnam(destination.Country))
            return 25_000m;

        return 85_000m; // Quốc tế
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
