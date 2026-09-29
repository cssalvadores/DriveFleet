namespace DriveFleet.Mobile.Models.Reservations;

/// <summary>
/// Represents an extra included
/// in a reserved vehicle.
/// </summary>
public class ReservationExtraModel
{
    public int ExtraId { get; set; }

    public string Name { get; set; } =
        string.Empty;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Total { get; set; }
}
