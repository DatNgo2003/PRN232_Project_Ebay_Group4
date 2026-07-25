namespace Backend.Services.Shipping;

/// <summary>
/// Resolves a specific shipping carrier by its key.
/// Similar pattern to IPaymentGatewayFactory.
/// </summary>
public interface IShippingCarrierFactory
{
    /// <summary>Get a carrier by key (e.g. "GHTK", "VIETTEL_POST", "JT_EXPRESS").</summary>
    IShippingCarrierService Resolve(string carrierKey);

    /// <summary>List all registered carriers.</summary>
    IReadOnlyList<ShippingCarrierInfo> GetAvailableCarriers();
}
