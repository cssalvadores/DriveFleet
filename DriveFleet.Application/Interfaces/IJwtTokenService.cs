using DriveFleet.Application.DTOs.Auth;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines operations for generating JWT access tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a signed JWT access token for an authenticated user.
    /// </summary>
    /// <param name="userId">
    /// The authenticated user's identifier.
    /// </param>
    /// <param name="email">
    /// The authenticated user's email address.
    /// </param>
    /// <param name="role">
    /// The authenticated user's role name.
    /// </param>
    /// <returns>
    /// The generated access token and its expiration date.
    /// </returns>
    JwtTokenResult GenerateToken(
        int userId,
        string email,
        string role);
}
