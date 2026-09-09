using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Entities;
using DriveFleet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriveFleet.Infrastructure.Repositories;

/// <summary>
/// Provides email confirmation token persistence operations
/// using Entity Framework Core.
/// </summary>
public class EmailConfirmationTokenRepository
    : IEmailConfirmationTokenRepository
{
    private readonly DriveFleetDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EmailConfirmationTokenRepository"/> class.
    /// </summary>
    /// <param name="dbContext">
    /// The database context used to access email confirmation token data.
    /// </param>
    public EmailConfirmationTokenRepository(
        DriveFleetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds a new email confirmation token and persists it to the database.
    /// </summary>
    /// <param name="token">
    /// The email confirmation token entity to be added.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    public async Task AddAsync(
        EmailConfirmationToken token,
        CancellationToken cancellationToken = default)
    {
        // Adds the token entity to the Entity Framework Core change tracker.
        await _dbContext.EmailConfirmationTokens.AddAsync(
            token,
            cancellationToken);

        // Persists the new token to the database.
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

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
    public async Task<EmailConfirmationToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailConfirmationTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);
    }

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
    public async Task ConfirmAsync(
        EmailConfirmationToken token,
        DateTime confirmedAt,
        CancellationToken cancellationToken = default)
    {
        // Marks the confirmation token as used.
        token.ConfirmedAt = confirmedAt;

        // Marks the associated user account as email-confirmed.
        token.User.EmailConfirmed = true;
        token.User.UpdatedAt = confirmedAt;

        // Persists the token and user changes in a single SaveChanges operation.
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
