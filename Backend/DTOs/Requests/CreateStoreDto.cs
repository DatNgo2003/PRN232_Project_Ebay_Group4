using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests
{
    public class CreateStoreDto
    {
        [Required]
        [MaxLength(100)]
        public string StoreName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? BannerImageUrl { get; set; }
    }

    public class UpdateStoreDto
    {
        [MaxLength(100)]
        public string? StoreName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? BannerImageUrl { get; set; }
    }
}
