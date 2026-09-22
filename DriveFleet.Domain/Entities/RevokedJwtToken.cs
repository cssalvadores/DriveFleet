namespace DriveFleet.Domain.Entities;

/// <summary>
/// Represents a JWT access token that has been revoked
/// before its normal expiration time.
/// </summary>
public class RevokedJwtToken
{
    /// <summary>
    /// Gets or sets the revoked token identifier.
    /// </summary>
    public int RevokedJwtTokenId { get; set; }

    /// <summary>
    /// Gets or sets the unique JWT identifier stored in the jti claim.
    /// </summary>
    public string Jti { get; set; } = null!;

    /// <summary>
    /// Gets or sets the UTC date and time when the JWT expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the JWT was revoked.
    /// </summary>
    public DateTime RevokedAt { get; set; }
}
