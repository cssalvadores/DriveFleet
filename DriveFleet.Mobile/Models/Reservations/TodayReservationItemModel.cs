namespace DriveFleet.Mobile.Models.Reservations;

/// <summary>
/// Represents a reservation prepared for display
/// in the employee daily operations list.
/// </summary>
public class TodayReservationItemModel
{
    public int ReservationId { get; set; }

    public string ReservationNumber =>
        $"Reservation #{ReservationId}";

    public string CustomerName { get; set; } =
        string.Empty;

    public string CustomerEmail { get; set; } =
        string.Empty;

    public string ReservationStatusName { get; set; } =
        string.Empty;

    public decimal TotalValue { get; set; }

    public int VehicleCount { get; set; }

    public string VehicleSummary { get; set; } =
        string.Empty;

    public string PeriodSummary { get; set; } =
        string.Empty;

    public string VehicleCountText =>
        VehicleCount == 1
            ? "1 vehicle"
            : $"{VehicleCount} vehicles";
}
