using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Dispute
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? RaisedBy { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public string? Resolution { get; set; }

    public DateTime? SubmittedDate { get; set; }

    public DateTime? SolvedDate { get; set; }

    public string? Comment { get; set; }

    public virtual OrderTable? Order { get; set; }

    public virtual User? RaisedByNavigation { get; set; }
}
