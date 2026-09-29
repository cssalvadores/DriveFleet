namespace DriveFleet.Mobile.Models.Vehicles;

/// <summary>
/// Represents a reservation entry displayed
/// in a vehicle reservation history.
/// </summary>
public class VehicleReservationHistoryItemModel
{
    /// <summary>
    /// Gets or sets the reservation identifier.
    /// </summary>
    public int ReservationId { get; set; }

    /// <summary>
    /// Gets the formatted reservation number.
    /// </summary>
    public string ReservationNumber =>
        $"Reservation #{ReservationId}";

    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    public string CustomerName { get; set; } =
        string.Empty;

    /// <summary>
    /// Gets or sets the reservation status.
    /// </summary>
    public string ReservationStatusName { get; set; } =
        string.Empty;

    /// <summary>
    /// Gets or sets the rental start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the rental end date.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the total value associated
    /// with the vehicle in this reservation.
    /// </summary>
    public decimal VehicleTotalValue { get; set; }

    /// <summary>
    /// Gets the formatted rental period.
    /// </summary>
    public string PeriodText =>
        $"{StartDate:dd MMM yyyy HH:mm} → " +
        $"{EndDate:dd MMM yyyy HH:mm}";
}