using Backend.Models;

namespace Backend.Services.Shipping;

/// <summary>
/// Represents a single shipping carrier (GHTK, Viettel Post, J&T Express, etc.).
/// Each implementation simulates the carrier's real API behavior.
/// </summary>
public interface IShippingCarrierService
{
    /// <summary>Unique carrier key used for DI lookup (e.g. "GHTK", "VIETTEL_POST").</summary>
    string CarrierKey { get; }

    /// <summary>Human-readable carrier name (e.g. "Giao Hàng Tiết Kiệm").</summary>
    string DisplayName { get; }

    /// <summary>Create a shipment with this carrier. Returns tracking number + estimated arrival.</summary>
    Task<CarrierShipmentResult> CreateShipmentAsync(
        Address destination,
        decimal orderTotal,
        string orderReference,
        CancellationToken cancellationToken = default);

    /// <summary>Update shipment status with this carrier.</summary>
    Task<bool> UpdateStatusAsync(
        string trackingNumber,
        string status,
        CancellationToken cancellationToken = default);

    /// <summary>Estimate delivery fee for a given destination.</summary>
    decimal EstimateFee(Address destination, decimal orderTotal);
}

public sealed record CarrierShipmentResult(
    string TrackingNumber,
    string Status,
    DateTime EstimatedArrival);
