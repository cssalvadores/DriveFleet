namespace DriveFleet.Application.DTOs.Auth;

/// <summary>
/// Represents the result of a successful authentication operation.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the authenticated user's identifier.
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
    /// Gets or sets the UTC date and time when the access token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
