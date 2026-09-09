using System.Security.Cryptography;
using System.Text;
using DriveFleet.Application.Interfaces;

namespace DriveFleet.Infrastructure.Security;

/// <summary>
/// Provides cryptographically secure token generation and hashing.
/// </summary>
public class SecureTokenService : ISecureTokenService
{
    private const int TokenSizeInBytes = 32;

    /// <summary>
    /// Generates a cryptographically secure random token.
    /// </summary>
    /// <returns>
    /// A 256-bit random token represented as a hexadecimal string.
    /// </returns>
    public string GenerateToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(
            TokenSizeInBytes);

        return Convert.ToHexString(tokenBytes);
    }

    /// <summary>
    /// Generates a SHA-256 hash for the specified token.
    /// </summary>
    /// <param name="token">
    /// The raw token to hash.
    /// </param>
    /// <returns>
    /// The hexadecimal SHA-256 hash of the token.
    /// </returns>
    public string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}
