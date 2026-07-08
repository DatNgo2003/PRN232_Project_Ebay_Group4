using Backend.DTOs.Responses; 
using Backend.Models;

namespace Backend.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProductsAsync();

        Task<ProductDetailDto?> GetProductDetailAsync(int id);
    }
}