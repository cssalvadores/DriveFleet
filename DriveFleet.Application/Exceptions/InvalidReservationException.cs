namespace DriveFleet.Application.Exceptions;

/// <summary>
/// Represents an error caused by invalid reservation data.
/// </summary>
public class InvalidReservationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="InvalidReservationException"/> class.
    /// </summary>
    /// <param name="message">The validation error message.</param>
    public InvalidReservationException(string message)
        : base(message)
    {
    }
}
