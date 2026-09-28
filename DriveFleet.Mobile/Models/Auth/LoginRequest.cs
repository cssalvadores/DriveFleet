namespace DriveFleet.Mobile.Models.Auth;

/// <summary>
/// Represents the credentials sent to the
/// DriveFleet authentication endpoint.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the employee email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the employee password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}