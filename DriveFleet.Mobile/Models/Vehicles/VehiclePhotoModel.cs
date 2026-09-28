namespace DriveFleet.Mobile.Models.Vehicles;

/// <summary>
/// Represents a catalog photo associated
/// with a DriveFleet vehicle.
/// </summary>
public class VehiclePhotoModel
{
    /// <summary>
    /// Gets or sets the vehicle photo identifier.
    /// </summary>
    public int VehiclePhotoId { get; set; }

    /// <summary>
    /// Gets or sets the relative file path
    /// returned by the DriveFleet API.
    /// </summary>
    public string FilePath { get; set; } =
        string.Empty;

    /// <summary>
    /// Gets or sets the catalog display order.
    /// </summary>
    public int DisplayOrder { get; set; }
}
