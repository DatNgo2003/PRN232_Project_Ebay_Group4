namespace Backend.Services.PaymentGateways;

/// <summary>
/// Registry plug-in: tự động gom mọi IPaymentGateway được đăng ký trong DI.
/// Thêm provider mới = thêm 1 class + 1 dòng AddScoped, factory tự nhận diện.
/// </summary>
public sealed class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly Dictionary<string, IPaymentGateway> _gateways;

    public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways.ToDictionary(g => g.ProviderName, StringComparer.OrdinalIgnoreCase);
    }

    public IPaymentGateway Resolve(string providerName)
    {
        if (_gateways.TryGetValue(providerName, out var gateway))
            return gateway;

        throw new NotSupportedException(
            $"Payment provider '{providerName}' chưa được đăng ký (plug-in). " +
            $"Provider hợp lệ: {string.Join(", ", _gateways.Keys)}");
    }
}