namespace Backend.DTOs.Responses;

public class CartDto
{
    public int CartId { get; set; }
    public int UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public int TotalItems { get; set; }
}

public class CartItemDto
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public string? ProductTitle { get; set; }
    public string? ProductImage { get; set; }
    public decimal? UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public int? StockQuantity { get; set; }
    public bool IsInStock { get; set; }
}
