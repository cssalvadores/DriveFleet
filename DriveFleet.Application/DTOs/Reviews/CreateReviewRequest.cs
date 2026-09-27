namespace DriveFleet.Application.DTOs.Reviews;

/// <summary>
/// Represents the information submitted when
/// creating a vehicle review.
/// </summary>
public class CreateReviewRequest
{
    public int Stars { get; set; }

    public string? Comment { get; set; }
}
