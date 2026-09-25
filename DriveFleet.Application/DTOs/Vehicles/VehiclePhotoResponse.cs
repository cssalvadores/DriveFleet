namespace DriveFleet.Application.DTOs.Vehicles;

/// <summary>
/// Represents a catalog photo associated with a vehicle.
/// </summary>
public class VehiclePhotoResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the vehicle photo.
    /// </summary>
    public int VehiclePhotoId { get; set; }

    /// <summary>
    /// Gets or sets the relative path of the stored photo.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display position of the photo.
    /// Position 1 represents the main catalog photo.
    /// </summary>
    public int DisplayOrder { get; set; }
}
