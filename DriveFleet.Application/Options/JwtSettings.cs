namespace DriveFleet.Application.Options;

/// <summary>
/// Represents configuration settings used to create and validate JWT tokens.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Gets or sets the issuer that creates the JWT.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the intended audience of the JWT.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the secret key used to sign the JWT.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT lifetime in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; }
}
