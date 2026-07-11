using Backend.Models;

namespace Backend.Services.Implementation
{
    public class SimpleRegionShippingFeeCalculator : IShippingFeeCalculator
    {
        private const decimal InnerCityFee = 0.8m;      
        private const decimal DomesticFee = 1.4m;       
        private const decimal InternationalFee = 6m;

        public decimal Calculate(Address address)
        {
            if (address.Country != null &&
                !address.Country.Equals("Vietnam", System.StringComparison.OrdinalIgnoreCase) &&
                !address.Country.Equals("Việt Nam", System.StringComparison.OrdinalIgnoreCase))
            {
                return InternationalFee;
            }

            if (RegionHelper.IsInnerCity(address.City))
            {
                return InnerCityFee;
            }

            return DomesticFee;
        }
    }
}