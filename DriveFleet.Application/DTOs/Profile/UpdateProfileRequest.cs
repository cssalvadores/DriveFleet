using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Application.DTOs.Profile;

/// <summary>
/// Represents a request to update
/// the authenticated user's profile.
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    [Required]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the user's phone number.
    /// </summary>
    [Phone]
    public string? Phone { get; set; }
}
