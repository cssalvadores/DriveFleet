namespace DriveFleet.Application.DTOs.Reviews;

/// <summary>
/// Represents a vehicle review returned by the application.
/// </summary>
public class ReviewResponse
{
    public int ReviewId { get; set; }

    public int ReservationVehicleId { get; set; }

    public int VehicleId { get; set; }

    public int Stars { get; set; }

    public string? Comment { get; set; }

    public bool IsVisible { get; set; }

    public DateTime CreatedAt { get; set; }
}
