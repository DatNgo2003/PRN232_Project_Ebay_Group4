using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests
{
    public class SellerReviewCreateDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; }
    }
}
