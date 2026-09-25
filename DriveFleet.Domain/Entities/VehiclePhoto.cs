namespace DriveFleet.Domain.Entities;

/// <summary>
/// Represents a catalog photo associated with a vehicle.
/// </summary>
public class VehiclePhoto
{
    /// <summary>
    /// Gets or sets the unique identifier of the vehicle photo.
    /// </summary>
    public int VehiclePhotoId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the vehicle
    /// associated with this photo.
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Gets or sets the relative path of the stored photo.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display position of the photo.
    /// Position 1 represents the main catalog photo.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when
    /// the photo record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the vehicle associated with this photo.
    /// </summary>
    public Vehicle Vehicle { get; set; } = null!;
}
