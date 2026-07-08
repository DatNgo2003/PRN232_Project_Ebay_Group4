using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class ReturnRequest
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? UserId { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual OrderTable? Order { get; set; }

    public virtual User? User { get; set; }
}
