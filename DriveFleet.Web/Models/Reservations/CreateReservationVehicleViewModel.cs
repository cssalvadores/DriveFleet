namespace DriveFleet.Web.Models.Reservations;

/// <summary>
/// Represents a vehicle available for inclusion in a new reservation.
/// </summary>
public class CreateReservationVehicleViewModel
{   

    /// <summary>
    /// Gets or sets the vehicle identifier.
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle brand.
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vehicle model.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vehicle license plate.
    /// </summary>
    public string LicensePlate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current daily price displayed to the client.
    /// </summary>
    public decimal DailyPrice { get; set; }

    /// <summary>
    /// Gets or sets the requested rental start date and time.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the requested rental end date and time.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the extras available for this vehicle.
    /// </summary>
    public List<CreateReservationExtraViewModel> Extras { get; set; }
    = new();
}
