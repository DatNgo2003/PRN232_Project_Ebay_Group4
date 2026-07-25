namespace Backend.DTOs.Responses
{
    public class StoreDto
    {
        public int Id { get; set; }
        public int SellerId { get; set; }
        public string? SellerUsername { get; set; }
        public string? StoreName { get; set; }
        public string? Description { get; set; }
        public string? BannerImageUrl { get; set; }
        public int TotalProducts { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalSales { get; set; }
    }
}
