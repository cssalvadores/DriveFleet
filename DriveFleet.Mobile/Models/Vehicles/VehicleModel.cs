namespace DriveFleet.Mobile.Models.Vehicles;

/// <summary>
/// Represents a vehicle returned by
/// the DriveFleet REST API.
/// </summary>
public class VehicleModel
{
    public int VehicleId { get; set; }

    public string Brand { get; set; } =
        string.Empty;

    public string Model { get; set; } =
        string.Empty;

    public int Year { get; set; }

    public string LicensePlate { get; set; } =
        string.Empty;

    public int Seats { get; set; }

    public decimal DailyPrice { get; set; }

    public List<VehiclePhotoModel> Photos { get; set; } =
        new();

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } =
        string.Empty;

    public int VehicleStatusId { get; set; }

    public string VehicleStatusName { get; set; } =
        string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}