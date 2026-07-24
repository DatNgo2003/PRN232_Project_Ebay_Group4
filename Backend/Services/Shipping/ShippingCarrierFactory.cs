namespace Backend.Services.Shipping;

/// <summary>
/// Factory that resolves shipping carriers by key.
/// Carriers are registered in DI as IShippingCarrierService.
/// </summary>
public sealed class ShippingCarrierFactory : IShippingCarrierFactory
{
    private readonly Dictionary<string, IShippingCarrierService> _carriers;

    public ShippingCarrierFactory(IEnumerable<IShippingCarrierService> carriers)
    {
        _carriers = carriers.ToDictionary(c => c.CarrierKey, StringComparer.OrdinalIgnoreCase);
    }

    public IShippingCarrierService Resolve(string carrierKey)
    {
        if (_carriers.TryGetValue(carrierKey, out var carrier))
            return carrier;

        throw new NotSupportedException(
            $"Shipping carrier '{carrierKey}' is not registered. " +
            $"Available carriers: {string.Join(", ", _carriers.Keys)}");
    }

    public IReadOnlyList<ShippingCarrierInfo> GetAvailableCarriers()
    {
        return _carriers.Values
            .Select(c => new ShippingCarrierInfo(c.CarrierKey, c.DisplayName, $"Ship with {c.DisplayName}"))
            .ToList()
            .AsReadOnly();
    }
}
