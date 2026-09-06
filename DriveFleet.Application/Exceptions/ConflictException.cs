namespace DriveFleet.Application.Exceptions;

/// <summary>
/// Represents a conflict between a requested operation
/// and the current state of the application.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
