namespace DriveFleet.Application.Exceptions;

/// <summary>
/// Represents an error caused by invalid reservation
/// filtering criteria.
/// </summary>
public class InvalidReservationFilterException : Exception
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="InvalidReservationFilterException"/> class.
    /// </summary>
    /// <param name="message">
    /// The error message describing the invalid filter.
    /// </param>
    public InvalidReservationFilterException(
        string message)
        : base(message)
    {
    }
}
