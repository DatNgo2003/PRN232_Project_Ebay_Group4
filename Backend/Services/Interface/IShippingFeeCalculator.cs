using Backend.Models;

public interface IShippingFeeCalculator
{
    decimal Calculate(Address address);
}

