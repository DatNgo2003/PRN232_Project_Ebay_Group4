using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public int? SellerId { get; set; }

    public decimal? AverageRating { get; set; }

    public int? TotalReviews { get; set; }

    public decimal? PositiveRate { get; set; }

    public int? OrdersId { get; set; }

    public virtual OrderTable? Orders { get; set; }

    public virtual User? Seller { get; set; }
}
