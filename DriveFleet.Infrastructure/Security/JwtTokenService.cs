using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DriveFleet.Application.DTOs.Auth;
using DriveFleet.Application.Interfaces;
using DriveFleet.Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DriveFleet.Infrastructure.Security;

/// <summary>
/// Generates signed JWT access tokens for authenticated users.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="options">
    /// JWT configuration settings used to create access tokens.
    /// </param>
    public JwtTokenService(
        IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

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
    public JwtTokenResult GenerateToken(
        int userId,
        string email,
        string role)
    {
        ValidateSettings();

        var now = DateTime.UtcNow;

        var expiresAt = now.AddMinutes(
            _settings.ExpirationMinutes);

        // Creates the identity and authorization claims stored in the JWT.
        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                userId.ToString()),

            new(
                ClaimTypes.Email,
                email),

            new(
                ClaimTypes.Role,
                role),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        // Creates the symmetric key used to sign the JWT.
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _settings.SecretKey));

        // Configures HMAC SHA-256 as the token signing algorithm.
        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        // Creates the JWT with issuer, audience, claims and expiration.
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        // Serializes the JWT into the compact string sent to the client.
        var tokenHandler = new JwtSecurityTokenHandler();

        var accessToken =
            tokenHandler.WriteToken(token);

        return new JwtTokenResult
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt
        };
    }

    /// <summary>
    /// Validates the JWT configuration required to generate tokens.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required JWT configuration is missing or invalid.
    /// </exception>
    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.Issuer))
        {
            throw new InvalidOperationException(
                "JWT issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.Audience))
        {
            throw new InvalidOperationException(
                "JWT audience is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            throw new InvalidOperationException(
                "JWT secret key is not configured.");
        }

        if (_settings.ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JWT expiration must be greater than zero.");
        }
    }
}
