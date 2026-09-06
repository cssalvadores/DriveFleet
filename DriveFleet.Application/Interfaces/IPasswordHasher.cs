namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines operations for securely hashing and verifying passwords.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
