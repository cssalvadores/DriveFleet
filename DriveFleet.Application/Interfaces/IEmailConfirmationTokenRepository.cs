using DriveFleet.Domain.Entities;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines persistence operations for email confirmation tokens.
/// </summary>
public interface IEmailConfirmationTokenRepository
{
    /// <summary>
    /// Adds a new email confirmation token and persists it to the data store.
    /// </summary>
    /// <param name="token">
    /// The email confirmation token entity to be added.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    Task AddAsync(
        EmailConfirmationToken token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an email confirmation token by its hashed value.
    /// </summary>
    /// <param name="tokenHash">
    /// The SHA-256 hash of the raw confirmation token.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    /// <returns>
    /// The matching email confirmation token, including its associated user,
    /// or null if no matching token exists.
    /// </returns>
    Task<EmailConfirmationToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an email confirmation token and its associated user
    /// as confirmed and persists the changes.
    /// </summary>
    /// <param name="token">
    /// The email confirmation token to confirm.
    /// </param>
    /// <param name="confirmedAt">
    /// The UTC date and time when the confirmation occurred.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    Task ConfirmAsync(
        EmailConfirmationToken token,
        DateTime confirmedAt,
        CancellationToken cancellationToken = default);

}
