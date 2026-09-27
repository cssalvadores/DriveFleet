namespace DriveFleet.Web.Models.Reservations;

/// <summary>
/// Represents a review displayed for a reserved vehicle.
/// </summary>
public class ReservationReviewViewModel
{
    public int ReviewId { get; set; }

    public int ReservationVehicleId { get; set; }

    public int VehicleId { get; set; }

    public int Stars { get; set; }

    public string? Comment { get; set; }

    public bool IsVisible { get; set; }

    public DateTime CreatedAt { get; set; }
}
