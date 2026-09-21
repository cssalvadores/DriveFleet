namespace DriveFleet.Application.DTOs.Profile;

/// <summary>
/// Represents the profile information of an authenticated user.
/// </summary>
public class UserProfileResponse
{
    /// <summary>
    /// Gets or sets the user's identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the user's profile photo value.
    /// </summary>
    public string? Photo { get; set; }

    /// <summary>
    /// Gets or sets the authentication provider associated with the user.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Gets or sets whether the user's email address is confirmed.
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Gets or sets the user's role.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
