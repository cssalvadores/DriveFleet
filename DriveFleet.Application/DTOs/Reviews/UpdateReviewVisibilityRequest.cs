namespace DriveFleet.Application.DTOs.Reviews;

/// <summary>
/// Represents an administrative review visibility update.
/// </summary>
public class UpdateReviewVisibilityRequest
{
    /// <summary>
    /// Gets or sets whether the review is publicly visible.
    /// </summary>
    public bool IsVisible { get; set; }
}
