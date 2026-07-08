using Backend.DTOs.Responses;
using Backend.Models;
using Backend.Repositories;
using System.Linq;

namespace Backend.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _productRepository.GetProductsAsync();
        }

        public async Task<ProductDetailDto?> GetProductDetailAsync(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                return null;
            }

            var dto = new ProductDetailDto
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                Images = product.Images,
                CategoryName = product.Category?.Name,
                    SellerName = product.Seller?.Username,
                    SellerId = product.Seller?.Id
            };

            var reviews = product.Reviews.ToList();
            dto.ReviewCount = reviews.Count;

            if (dto.ReviewCount > 0)
            {
                dto.AverageRating = reviews.Average(r => r.Rating ?? 0);

                dto.Reviews = reviews.Select(r => new ReviewDto
                {
                    Rating = r.Rating ?? 0,
                    Comment = r.Comment,
                    ReviewerUsername = r.Reviewer?.Username ?? "Anonymous",
                    CreatedAt = r.CreatedAt ?? DateTime.MinValue
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

                dto.RatingCounts = reviews
                    .GroupBy(r => r.Rating ?? 0)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            else
            {
                dto.AverageRating = 0;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (!dto.RatingCounts.ContainsKey(i))
                {
                    dto.RatingCounts.Add(i, 0);
                }
            }

            return dto;
        }
    }
}