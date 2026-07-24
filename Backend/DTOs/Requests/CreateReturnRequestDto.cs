using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests
{
    public class CreateReturnRequestDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdateReturnStatusDto
    {
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;
    }
}
