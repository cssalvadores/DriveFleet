namespace DriveFleet.Web.Models.Vehicles;

/// <summary>
/// Represents a vehicle catalog photo displayed by the Web application.
/// </summary>
public class VehiclePhotoViewModel
{
    /// <summary>
    /// Gets or sets the identifier of the vehicle photo.
    /// </summary>
    public int VehiclePhotoId { get; set; }

    /// <summary>
    /// Gets or sets the relative path of the stored vehicle photo.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the absolute public URL used to display
    /// the vehicle photo in the Web application.
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the catalog display position of the photo.
    /// Position 1 represents the main catalog photo.
    /// </summary>
    public int DisplayOrder { get; set; }
}
