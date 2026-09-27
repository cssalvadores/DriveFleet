namespace DriveFleet.Web.Models.Reservations;

/// <summary>
/// Represents a vehicle included in a reservation.
/// </summary>
public class ReservationVehicleViewModel
{
    /// <summary>
    /// Gets or sets the reservation vehicle identifier.
    /// </summary>
    public int ReservationVehicleId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle identifier.
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle brand.
    /// </summary>
    public string VehicleBrand { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vehicle model.
    /// </summary>
    public string VehicleModel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vehicle license plate.
    /// </summary>
    public string LicensePlate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rental start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the rental end date.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the daily price applied to the reservation.
    /// </summary>
    public decimal DailyPrice { get; set; }

    /// <summary>
    /// Gets or sets the number of billable rental days.
    /// </summary>
    public int RentalDays { get; set; }

    /// <summary>
    /// Gets or sets the rental value before extras.
    /// </summary>
    public decimal RentalValue { get; set; }

    /// <summary>
    /// Gets or sets the total value of the selected extras.
    /// </summary>
    public decimal ExtrasValue { get; set; }

    /// <summary>
    /// Gets or sets the total value for this reserved vehicle.
    /// </summary>
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Gets or sets the extras associated with this vehicle.
    /// </summary>
    public ICollection<ReservationExtraViewModel> Extras { get; set; }
        = new List<ReservationExtraViewModel>();

    /// <summary>
    /// Gets or sets the review created for this
    /// reserved vehicle, when available.
    /// </summary>
    public ReservationReviewViewModel? Review { get; set; }
}
