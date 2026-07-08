using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public class SellerLeaveReviewViewModel
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}
