using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services
{
    public interface IStoreService
    {
        Task<StoreDto?> CreateStoreAsync(string sellerUsername, CreateStoreDto dto);
        Task<StoreDto?> UpdateStoreAsync(string sellerUsername, UpdateStoreDto dto);
        Task<StoreDto?> GetMyStoreAsync(string sellerUsername);
        Task<StoreDto?> GetStoreBySellerIdAsync(int sellerId);
        Task<StoreDto?> GetStoreByIdAsync(int storeId);
        Task<IEnumerable<StoreDto>> GetAllStoresAsync();
    }
}
