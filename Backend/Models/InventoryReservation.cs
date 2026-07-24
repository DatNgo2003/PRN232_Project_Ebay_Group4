namespace Backend.Models;

public sealed class InventoryReservation
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; } = InventoryReservationStatus.Held;

    public DateTime CreatedAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }

    public OrderTable Order { get; set; } = null!;

    public Product Product { get; set; } = null!;
}

public static class InventoryReservationStatus
{
    public const string Held = "Held";
    public const string Confirmed = "Confirmed";
    public const string Released = "Released";
}
