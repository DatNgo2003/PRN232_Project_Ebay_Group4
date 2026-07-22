namespace Backend.Services.PaymentGateways;

public interface IPaymentGatewayFactory
{
    IPaymentGateway Resolve(string providerName);
}