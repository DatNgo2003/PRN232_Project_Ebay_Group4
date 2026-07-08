using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class DetailFeedback
{
    public int Id { get; set; }

    public int? DeliveryOnTime { get; set; }

    public int? ExactSame { get; set; }

    public int? Communication { get; set; }

    public int FeedbackId { get; set; }

    public virtual Feedback Feedback { get; set; } = null!;
}
