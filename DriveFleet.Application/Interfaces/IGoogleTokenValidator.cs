using DriveFleet.Application.DTOs.Auth;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines Google OAuth token validation operations.
/// </summary>
public interface IGoogleTokenValidator
{
    /// <summary>
    /// Validates a Google OAuth access token
    /// and returns the associated Google identity.
    /// </summary>
    Task<GoogleIdentity?> ValidateAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}
