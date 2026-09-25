namespace DriveFleet.Web.Models.Vehicles;

/// <summary>
/// Represents a vehicle displayed in the public vehicle list.
/// </summary>
public class VehicleListItemViewModel
{
    public int VehicleId { get; set; }

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public int Seats { get; set; }

    public decimal DailyPrice { get; set; }    

    /// <summary>
    /// Gets or sets the absolute URL of the main catalog photo.
    /// </summary>
    public string? MainPhotoUrl { get; set; }

    public string? Description { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string VehicleStatusName { get; set; } = string.Empty;
}
