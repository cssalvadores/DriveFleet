namespace DriveFleet.Mobile.Models.Auth;

/// <summary>
/// Represents the authenticated employee session
/// stored securely by the mobile application.
/// </summary>
public class EmployeeSession
{
    /// <summary>
    /// Gets or sets the authenticated user identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the employee first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the employee last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the employee email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authenticated role.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC date and time
    /// when the access token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
