using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Abstraction for the shipping carrier API.  The application currently uses
/// <see cref="MockShippingService"/>, but the order flow does not depend on the
/// carrier implementation and can therefore be switched to a real carrier later.
/// </summary>
public interface IShippingService
{
    Task<ShippingShipment> CreateShipmentAsync(
        Address destination,
        DateTime estimatedArrival,
        string orderReference,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateShipmentStatusAsync(
        string trackingNumber,
        string status,
        CancellationToken cancellationToken = default);
}

public sealed record ShippingShipment(
    string Carrier,
    string TrackingNumber,
    string Status,
    DateTime EstimatedArrival);
