using Backend.Models;

namespace Backend.DTOs.Responses
{
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Images { get; set; }
        public string? CategoryName { get; set; }
        public string? SellerName { get; set; }
        public int? SellerId { get; set; }

        public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        public Dictionary<int, int> RatingCounts { get; set; } = new Dictionary<int, int>();
    }
}