namespace DriveFleet.Application.DTOs.Reservations;

public class ReservationVehicleResponse
{
    public int ReservationVehicleId { get; set; }

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

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal DailyPrice { get; set; }

    /// <summary>
    /// Gets or sets the number of billable rental days.
    /// </summary>
    public int RentalDays { get; set; }

    /// <summary>
    /// Gets or sets the vehicle rental value before extras.
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

    public ICollection<ReservationExtraResponse> Extras { get; set; }
        = new List<ReservationExtraResponse>();
}
