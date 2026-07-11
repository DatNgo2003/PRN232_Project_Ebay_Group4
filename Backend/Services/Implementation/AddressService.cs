using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Exceptions;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IUserRepository _userRepository;

        public AddressService(IAddressRepository addressRepository, IUserRepository userRepository)
        {
            _addressRepository = addressRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<AddressDto>> GetMyAddressesAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null) return new List<AddressDto>();

            var addresses = await _addressRepository.GetByUserIdAsync(user.Id);
            return addresses.Select(MapToDto);
        }

        public async Task<AddressDto> CreateAddressAsync(string username, CreateAddressDto dto)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                throw new BusinessException("User not found.");

            if (string.IsNullOrWhiteSpace(dto.Street) || string.IsNullOrWhiteSpace(dto.City))
                throw new BusinessException("Please enter the full street and city.");

            var existing = await _addressRepository.GetByUserIdAsync(user.Id);
            var isFirstAddress = !existing.Any();

            var shouldBeDefault = isFirstAddress || dto.IsDefault;

            if (shouldBeDefault)
            {
                await _addressRepository.UnsetDefaultForUserAsync(user.Id);
            }

            var address = new Address
            {
                UserId = user.Id,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Street = dto.Street,
                City = dto.City,
                State = dto.State,
                Country = dto.Country,
                IsDefault = shouldBeDefault
            };

            var created = await _addressRepository.CreateAsync(address);
            return MapToDto(created);
        }

        private static AddressDto MapToDto(Address address)
        {
            return new AddressDto
            {
                Id = address.Id,
                FullName = address.FullName,
                Phone = address.Phone,
                Street = address.Street,
                City = address.City,
                State = address.State,
                Country = address.Country,
                IsDefault = address.IsDefault ?? false
            };
        }
    }
}