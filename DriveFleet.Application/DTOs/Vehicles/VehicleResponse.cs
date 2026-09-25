namespace DriveFleet.Application.DTOs.Vehicles;

/// <summary>
/// Represents vehicle information returned to API clients.
/// </summary>
public class VehicleResponse
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
    public List<VehiclePhotoResponse> Photos { get; set; } = [];

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int VehicleStatusId { get; set; }

    public string VehicleStatusName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
