using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class UpdateCartItemDto
{
    [Required]
    [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
    public int Quantity { get; set; }
}
