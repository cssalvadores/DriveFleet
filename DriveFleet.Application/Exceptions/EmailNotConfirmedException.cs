namespace DriveFleet.Application.Exceptions;

/// <summary>
/// Represents an authentication attempt using an account
/// whose email address has not yet been confirmed.
/// </summary>
public class EmailNotConfirmedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EmailNotConfirmedException"/> class.
    /// </summary>
    /// <param name="message">
    /// The message that describes the email confirmation requirement.
    /// </param>
    public EmailNotConfirmedException(string message)
        : base(message)
    {
    }
}
