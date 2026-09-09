namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines operations for sending application emails.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    /// <param name="recipientEmail">
    /// The recipient email address.
    /// </param>
    /// <param name="subject">
    /// The email subject.
    /// </param>
    /// <param name="htmlBody">
    /// The HTML content of the email message.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
