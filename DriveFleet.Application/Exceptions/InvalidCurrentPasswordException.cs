namespace DriveFleet.Application.Exceptions;

/// <summary>
/// Represents an error caused by an incorrect current password.
/// </summary>
public sealed class InvalidCurrentPasswordException : Exception
{
    public InvalidCurrentPasswordException()
    {
    }

    public InvalidCurrentPasswordException(
        string message)
        : base(message)
    {
    }

    public InvalidCurrentPasswordException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
