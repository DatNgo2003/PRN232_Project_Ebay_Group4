namespace Backend.Services.Shipping;

/// <summary>
/// Lightweight DTO returned by the /api/shipping/carriers endpoint
/// so the frontend can display available carriers.
/// </summary>
public sealed record ShippingCarrierInfo(
    string Key,
    string Name,
    string Description);
