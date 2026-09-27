namespace DriveFleet.Web.Models.Reservations;

/// <summary>
/// Represents an extra associated with a vehicle in a reservation.
/// </summary>
public class ReservationExtraViewModel
{
    /// <summary>
    /// Gets or sets the extra identifier.
    /// </summary>
    public int ExtraId { get; set; }

    /// <summary>
    /// Gets or sets the extra name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price applied to the reservation.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the total value for this extra.
    /// </summary>
    public decimal Total { get; set; }
}
