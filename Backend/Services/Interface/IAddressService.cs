using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetMyAddressesAsync(string username);
        Task<AddressDto> CreateAddressAsync(string username, CreateAddressDto dto);
    }
}