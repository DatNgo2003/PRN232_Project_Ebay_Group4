using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly CloneEbayDbContext _context;

        public AddressRepository(CloneEbayDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Address>> GetByUserIdAsync(int userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.Id)
                .ToListAsync();
        }

        public async Task<Address?> GetByIdAsync(int addressId)
        {
            return await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId);
        }

        public async Task<Address?> GetDefaultByUserIdAsync(int userId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault == true);
        }

        public async Task<Address> CreateAsync(Address address)
        {
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task UnsetDefaultForUserAsync(int userId)
        {
            var defaults = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault == true)
                .ToListAsync();

            foreach (var addr in defaults)
            {
                addr.IsDefault = false;
            }

            await _context.SaveChangesAsync();
        }
    }
}