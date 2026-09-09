namespace DriveFleet.Web.Models.Auth;

/// <summary>
/// Represents the possible states displayed by the email confirmation page.
/// </summary>
public enum ConfirmEmailViewState
{
    Success,
    AlreadyConfirmed,
    Error
}

/// <summary>
/// Represents the data displayed on the email confirmation page.
/// </summary>
public class ConfirmEmailViewModel
{
    /// <summary>
    /// Gets or sets the current confirmation result state.
    /// </summary>
    public ConfirmEmailViewState State { get; set; }

    /// <summary>
    /// Gets or sets the message displayed to the user.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
