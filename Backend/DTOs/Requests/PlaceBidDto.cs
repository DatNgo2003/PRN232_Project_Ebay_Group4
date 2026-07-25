using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests
{
    public class PlaceBidDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Số tiền đặt phải lớn hơn 0")]
        public decimal Amount { get; set; }
    }
}
