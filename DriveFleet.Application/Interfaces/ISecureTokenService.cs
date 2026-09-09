namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines operations for generating and hashing secure tokens.
/// </summary>
public interface ISecureTokenService
{
    /// <summary>
    /// Generates a cryptographically secure random token.
    /// </summary>
    /// <returns>
    /// A secure token suitable for use in confirmation and recovery flows.
    /// </returns>
    string GenerateToken();

    /// <summary>
    /// Generates a SHA-256 hash for the specified token.
    /// </summary>
    /// <param name="token">
    /// The raw token to hash.
    /// </param>
    /// <returns>
    /// The hexadecimal SHA-256 hash of the token.
    /// </returns>
    string HashToken(string token);
}
