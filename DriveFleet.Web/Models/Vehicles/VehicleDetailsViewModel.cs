namespace DriveFleet.Web.Models.Vehicles;

/// <summary>
/// Represents the information displayed on a vehicle details page.
/// </summary>
public class VehicleDetailsViewModel
{
    public int VehicleId { get; set; }

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public string LicensePlate { get; set; } = string.Empty;

    public int Seats { get; set; }

    public decimal DailyPrice { get; set; }
        
    /// <summary>
    /// Gets or sets the catalog photos associated with the vehicle.
    /// </summary>
    public List<VehiclePhotoViewModel> Photos { get; set; } = new();

    public string? Description { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string VehicleStatusName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the publicly visible reviews
    /// associated with the vehicle.
    /// </summary>
    public ICollection<VehicleReviewViewModel> Reviews { get; set; }
        = new List<VehicleReviewViewModel>();

    /// <summary>
    /// Gets the average rating of the visible reviews,
    /// or null when the vehicle has no reviews.
    /// </summary>
    public double? AverageRating =>
        Reviews.Count == 0
            ? null
            : Reviews.Average(review =>
                review.Stars);
}
