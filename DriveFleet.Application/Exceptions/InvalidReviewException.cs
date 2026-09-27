namespace DriveFleet.Application.Exceptions;

/// <summary>
/// Represents invalid vehicle review information.
/// </summary>
public class InvalidReviewException : Exception
{
    public InvalidReviewException(
        string message)
        : base(message)
    {
    }
}
