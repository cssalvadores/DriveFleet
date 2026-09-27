namespace DriveFleet.Web.Models.Reservations;

/// <summary>
/// Represents a vehicle that can be added to an existing
/// reservation creation request.
/// </summary>
public class ReservationVehicleOptionViewModel
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
    /// Gets or sets the vehicle year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the vehicle license plate.
    /// </summary>
    public string LicensePlate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of available seats.
    /// </summary>
    public int Seats { get; set; }

    /// <summary>
    /// Gets or sets the vehicle category name.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current daily rental price.
    /// </summary>
    public decimal DailyPrice { get; set; }

    /// <summary>
    /// Gets or sets the absolute URL of the vehicle's main catalog photo.
    /// </summary>
    public string? MainPhotoUrl { get; set; }
}
