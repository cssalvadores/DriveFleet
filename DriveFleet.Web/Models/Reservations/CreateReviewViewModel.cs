using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Web.Models.Reservations;

/// <summary>
/// Represents the review submitted by a client.
/// </summary>
public class CreateReviewViewModel
{
    /// <summary>
    /// Gets or sets the rating between one and five stars.
    /// </summary>
    [Range(
        1,
        5,
        ErrorMessage =
            "The rating must be between 1 and 5 stars.")]
    public int Stars { get; set; }

    /// <summary>
    /// Gets or sets the optional review comment.
    /// </summary>
    [StringLength(
        1000,
        ErrorMessage =
            "The comment cannot exceed 1000 characters.")]
    public string? Comment { get; set; }
}
