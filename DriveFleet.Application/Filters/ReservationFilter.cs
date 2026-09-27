namespace DriveFleet.Application.Filters;

/// <summary>
/// Represents validated reservation filtering criteria
/// used by the persistence layer.
/// </summary>
public sealed class ReservationFilter
{
    /// <summary>
    /// Gets or initializes the reservation status identifier.
    /// </summary>
    public int? StatusId { get; init; }

    /// <summary>
    /// Gets or initializes the first rental date.
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Gets or initializes the last rental date.
    /// </summary>
    public DateTime? ToDate { get; init; }
}
