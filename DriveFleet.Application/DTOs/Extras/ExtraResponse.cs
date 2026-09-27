namespace DriveFleet.Application.DTOs.Extras;

/// <summary>
/// Represents an extra available in the DriveFleet catalog.
/// </summary>
public class ExtraResponse
{
    /// <summary>
    /// Gets or sets the extra identifier.
    /// </summary>
    public int ExtraId { get; set; }

    /// <summary>
    /// Gets or sets the extra name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extra description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the current unit price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the relative photo path.
    /// </summary>
    public string? Photo { get; set; }
}
