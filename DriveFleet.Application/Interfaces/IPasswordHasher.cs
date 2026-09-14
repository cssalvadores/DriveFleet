namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines operations for securely hashing and verifying passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Creates a secure hash from a plain-text password.
    /// </summary>
    /// <param name="password">
    /// The plain-text password to hash.
    /// </param>
    /// <returns>
    /// The generated password hash.
    /// </returns>
    string Hash(string password);

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
    bool Verify(
        string hashedPassword,
        string providedPassword);
}
