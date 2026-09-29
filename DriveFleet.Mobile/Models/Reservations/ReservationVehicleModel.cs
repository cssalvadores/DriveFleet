namespace DriveFleet.Mobile.Models.Reservations;

/// <summary>
/// Represents a vehicle included in
/// a DriveFleet reservation.
/// </summary>
public class ReservationVehicleModel
{
    public int ReservationVehicleId { get; set; }

    public int VehicleId { get; set; }

    public string VehicleBrand { get; set; } =
        string.Empty;

    public string VehicleModel { get; set; } =
        string.Empty;

    public string LicensePlate { get; set; } =
        string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal DailyPrice { get; set; }

    public int RentalDays { get; set; }

    public decimal RentalValue { get; set; }

    public decimal ExtrasValue { get; set; }

    public decimal TotalValue { get; set; }

    public List<ReservationExtraModel> Extras { get; set; } =
        new();

    /// <summary>
    /// Gets the vehicle display name.
    /// </summary>
    public string DisplayName =>
        $"{VehicleBrand} {VehicleModel}";

    /// <summary>
    /// Gets the formatted rental period.
    /// </summary>
    public string PeriodText =>
        $"{StartDate:dd MMM yyyy HH:mm} → " +
        $"{EndDate:dd MMM yyyy HH:mm}";

    /// <summary>
    /// Gets a readable extras summary.
    /// </summary>
    public string ExtrasSummary =>
        Extras.Count == 0
            ? "No extras"
            : string.Join(
                ", ",
                Extras.Select(extra =>
                    $"{extra.Name} x{extra.Quantity}"));
}
