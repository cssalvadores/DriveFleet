namespace DriveFleet.Application.DTOs.Auth;

/// <summary>
/// Represents the identity of the currently authenticated user.
/// </summary>
public class CurrentUserResponse
{
    /// <summary>
    /// Gets or sets the authenticated user's identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authenticated user's role.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
