namespace DriveFleet.Application.Exceptions;

/// <summary>
/// Represents an authentication failure caused by invalid credentials.
/// </summary>
public class InvalidCredentialsException : Exception
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="InvalidCredentialsException"/> class.
    /// </summary>
    /// <param name="message">
    /// The message that describes the authentication failure.
    /// </param>
    public InvalidCredentialsException(string message)
        : base(message)
    {
    }
}
