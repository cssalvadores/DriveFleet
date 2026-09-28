namespace DriveFleet.Web.Models.Reviews;

/// <summary>
/// Represents a vehicle review displayed
/// in the administrative moderation interface.
/// </summary>
public class AdminReviewViewModel
{
    /// <summary>
    /// Gets or sets the review identifier.
    /// </summary>
    public int ReviewId { get; set; }

    /// <summary>
    /// Gets or sets the reserved vehicle identifier
    /// associated with the review.
    /// </summary>
    public int ReservationVehicleId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle identifier.
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Gets or sets the review rating.
    /// </summary>
    public int Stars { get; set; }

    /// <summary>
    /// Gets or sets the optional written comment.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets whether the review
    /// is publicly visible.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets the date and time
    /// when the review was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
