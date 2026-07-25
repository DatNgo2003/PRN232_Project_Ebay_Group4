using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class AddToCartDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ProductId phải lớn hơn 0")]
    public int ProductId { get; set; }

    [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
    public int Quantity { get; set; } = 1;
}
