using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class SellerToBuyerReview
{
    public int Id { get; set; }

    public int SellerId { get; set; }

    public string SellerName { get; set; } = null!;

    public int BuyerId { get; set; }

    public string BuyerName { get; set; } = null!;

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public int OrderId { get; set; }

    public string ProductName { get; set; } = null!;
}
