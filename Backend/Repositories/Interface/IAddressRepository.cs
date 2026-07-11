using Backend.Models;

namespace Backend.Repositories
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetByUserIdAsync(int userId);
        Task<Address?> GetByIdAsync(int addressId);
        Task<Address?> GetDefaultByUserIdAsync(int userId);
        Task<Address> CreateAsync(Address address);
        Task UnsetDefaultForUserAsync(int userId);
    }
}