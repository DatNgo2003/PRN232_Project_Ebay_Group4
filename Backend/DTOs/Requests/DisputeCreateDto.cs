using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests
{
    public class DisputeCreateDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [MinLength(20)]
        public string Description { get; set; }
    }
}
