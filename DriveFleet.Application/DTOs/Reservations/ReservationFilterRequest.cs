namespace DriveFleet.Application.DTOs.Reservations;

/// <summary>
/// Represents the optional filters used when retrieving reservations.
/// </summary>
public class ReservationFilterRequest
{
    /// <summary>
    /// Gets or sets the reservation status identifier.
    /// </summary>
    public int? StatusId { get; set; }

    /// <summary>
    /// Gets or sets the first date of the rental period filter.
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Gets or sets the last date of the rental period filter.
    /// </summary>
    public DateTime? ToDate { get; set; }
}
