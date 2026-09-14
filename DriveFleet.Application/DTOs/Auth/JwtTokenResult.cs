namespace DriveFleet.Application.DTOs.Auth;

/// <summary>
/// Represents the result of a JWT generation operation.
/// </summary>
public class JwtTokenResult
{
    /// <summary>
    /// Gets or sets the generated JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC date and time when the token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
