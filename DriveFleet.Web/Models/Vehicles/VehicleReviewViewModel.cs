namespace DriveFleet.Web.Models.Vehicles;

/// <summary>
/// Represents a publicly visible vehicle review.
/// </summary>
public class VehicleReviewViewModel
{
    /// <summary>
    /// Gets or sets the review identifier.
    /// </summary>
    public int ReviewId { get; set; }

    /// <summary>
    /// Gets or sets the rating from one to five stars.
    /// </summary>
    public int Stars { get; set; }

    /// <summary>
    /// Gets or sets the optional review comment.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the review creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
