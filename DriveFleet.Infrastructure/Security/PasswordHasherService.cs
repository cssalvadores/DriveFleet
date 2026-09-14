using DriveFleet.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DriveFleet.Infrastructure.Security;

/// <summary>
/// Provides password hashing and verification using
/// ASP.NET Core Identity password hashing.
/// </summary>
public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<object> _passwordHasher;
    private readonly object _userContext;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="PasswordHasherService"/> class.
    /// </summary>
    public PasswordHasherService()
    {
        _passwordHasher = new PasswordHasher<object>();
        _userContext = new object();
    }

    /// <summary>
    /// Creates a secure hash from a plain-text password.
    /// </summary>
    /// <param name="password">
    /// The plain-text password to hash.
    /// </param>
    /// <returns>
    /// The generated password hash.
    /// </returns>
    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(
            _userContext,
            password);
    }

    /// <summary>
    /// Verifies whether a plain-text password matches a stored password hash.
    /// </summary>
    /// <param name="hashedPassword">
    /// The password hash stored for the user.
    /// </param>
    /// <param name="providedPassword">
    /// The plain-text password provided during authentication.
    /// </param>
    /// <returns>
    /// True when the provided password matches the stored hash;
    /// otherwise, false.
    /// </returns>
    public bool Verify(
        string hashedPassword,
        string providedPassword)
    {
        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                _userContext,
                hashedPassword,
                providedPassword);

        // Both successful Identity results mean that the password is valid.
        return verificationResult !=
            PasswordVerificationResult.Failed;
    }
}
