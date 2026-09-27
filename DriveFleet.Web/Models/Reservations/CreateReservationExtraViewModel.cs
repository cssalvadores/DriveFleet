namespace DriveFleet.Web.Models.Reservations;

/// <summary>
/// Represents an extra that can be selected for a reserved vehicle.
/// </summary>
public class CreateReservationExtraViewModel
{
    /// <summary>
    /// Gets or sets the extra identifier.
    /// </summary>
    public int ExtraId { get; set; }

    /// <summary>
    /// Gets or sets the extra name displayed to the client.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current unit price displayed to the client.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the quantity selected by the client.
    /// Zero means that the extra is not selected.
    /// </summary>
    public int Quantity { get; set; }
}
