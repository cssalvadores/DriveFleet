namespace DriveFleet.Mobile.Models.Auth;

/// <summary>
/// Represents a successful authentication response
/// returned by the DriveFleet API.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the authenticated user identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authenticated user's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authenticated user's role.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC expiration date
    /// of the JWT access token.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
