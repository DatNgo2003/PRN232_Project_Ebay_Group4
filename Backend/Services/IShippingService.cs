using Backend.Models;
using Backend.Services.Shipping;

namespace Backend.Services;

/// <summary>
/// Abstraction for the shipping carrier API. Supports multiple carriers
/// (GHTK, Viettel Post, J&T Express) via <see cref="IShippingCarrierFactory"/>.
/// </summary>
public interface IShippingService
{
    Task<ShippingShipment> CreateShipmentAsync(
        Address destination,
        DateTime estimatedArrival,
        string orderReference,
        string? carrierKey = null,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateShipmentStatusAsync(
        string trackingNumber,
        string status,
        CancellationToken cancellationToken = default);

    /// <summary>Get list of available shipping carriers.</summary>
    IReadOnlyList<ShippingCarrierInfo> GetAvailableCarriers();

    /// <summary>Estimate shipping fee for a specific carrier and destination.</summary>
    decimal EstimateFee(Address destination, decimal orderTotal, string carrierKey);
}

public sealed record ShippingShipment(
    string Carrier,
    string TrackingNumber,
    string Status,
    DateTime EstimatedArrival);
