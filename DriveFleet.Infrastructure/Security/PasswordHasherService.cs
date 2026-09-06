using DriveFleet.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DriveFleet.Infrastructure.Security;

/// <summary>
/// Provides secure password hashing and verification using ASP.NET Core Identity.
/// </summary>
public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<object> _passwordHasher = new();
    private readonly object _userContext = new();

    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(
            _userContext,
            password);
    }

    public bool Verify(string password, string passwordHash)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            _userContext,
            passwordHash,
            password);

        return result != PasswordVerificationResult.Failed;
    }
}
