namespace DriveFleet.Application.Exceptions;

/// <summary>
/// Represents an error caused by an invalid vehicle-related reference,
/// such as a category or vehicle status that does not exist.
/// </summary>
public class InvalidVehicleReferenceException : Exception
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="InvalidVehicleReferenceException"/> class.
    /// </summary>
    /// <param name="message">
    /// The error message that describes the invalid reference.
    /// </param>
    public InvalidVehicleReferenceException(
        string message)
        : base(message)
    {
    }
}
