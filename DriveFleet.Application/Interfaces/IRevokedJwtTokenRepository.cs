using DriveFleet.Domain.Entities;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines persistence operations for revoked JWT access tokens.
/// </summary>
public interface IRevokedJwtTokenRepository
{
    /// <summary>
    /// Determines whether a JWT identifier has been revoked.
    /// </summary>
    /// <param name="jti">
    /// The unique identifier stored in the JWT jti claim.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    /// <returns>
    /// True if the JWT has been revoked; otherwise, false.
    /// </returns>
    Task<bool> IsRevokedAsync(
        string jti,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a revoked JWT token to the data store.
    /// </summary>
    /// <param name="token">
    /// The revoked JWT token information to persist.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    Task AddAsync(
        RevokedJwtToken token,
        CancellationToken cancellationToken = default);
}
