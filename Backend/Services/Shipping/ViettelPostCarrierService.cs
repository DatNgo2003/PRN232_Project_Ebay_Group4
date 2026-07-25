using System.Collections.Concurrent;
using Backend.Models;

namespace Backend.Services.Shipping;

/// <summary>
/// Simulates Viettel Post (VTP) shipping API.
/// Tracking format: VTP-XXXXXXXXXX (10 digits).
/// Fee: 18k VND base domestic, 30k inter-regional, 120k international.
/// Estimated delivery: 1-3 days domestic, 5-7 days inter-regional.
/// </summary>
public sealed class ViettelPostCarrierService : IShippingCarrierService
{
    private static readonly ConcurrentDictionary<string, string> _statuses = new(StringComparer.OrdinalIgnoreCase);

    public string CarrierKey => "VIETTEL_POST";
    public string DisplayName => "Viettel Post";

    public Task<CarrierShipmentResult> CreateShipmentAsync(
        Address destination,
        decimal orderTotal,
        string orderReference,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Viettel Post tracking: 10-digit number
        var random = new Random();
        var trackingNumber = $"VTP-{random.Next(1000000000, 1999999999)}";
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
        // Viettel Post: không free ship, phí cố định theo vùng
        if (IsHanoiOrHcm(destination.City))
            return 18_000m;

        if (IsVietnam(destination.Country))
            return 30_000m;

        return 120_000m; // Quốc tế
    }

    private static int GetEstimatedDays(Address destination)
    {
        if (IsHanoiOrHcm(destination.City)) return 1; // Viettel nhanh hơn trong nội thành
        if (IsVietnam(destination.Country)) return 3;
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
