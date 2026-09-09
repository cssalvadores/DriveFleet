namespace DriveFleet.Application.Exceptions;

/// <summary>
/// Represents an invalid or expired security token.
/// </summary>
public class InvalidTokenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="InvalidTokenException"/> class.
    /// </summary>
    /// <param name="message">
    /// The message that describes the token validation error.
    /// </param>
    public InvalidTokenException(string message)
        : base(message)
    {
    }
}
