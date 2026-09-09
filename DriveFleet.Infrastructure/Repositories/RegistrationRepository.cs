using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Entities;
using DriveFleet.Infrastructure.Data;

namespace DriveFleet.Infrastructure.Repositories;

/// <summary>
/// Provides persistence operations required during user registration.
/// </summary>
public class RegistrationRepository : IRegistrationRepository
{
    private readonly DriveFleetDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="RegistrationRepository"/> class.
    /// </summary>
    /// <param name="dbContext">
    /// The database context used to persist registration data.
    /// </param>
    public RegistrationRepository(
        DriveFleetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Persists a new user and the corresponding email confirmation token
    /// as a single database operation.
    /// </summary>
    /// <param name="user">
    /// The user entity to be created.
    /// </param>
    /// <param name="confirmationToken">
    /// The email confirmation token associated with the new user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    public async Task AddUserWithConfirmationTokenAsync(
        User user,
        EmailConfirmationToken confirmationToken,
        CancellationToken cancellationToken = default)
    {
        // Establishes the relationship before either entity is persisted.
        confirmationToken.User = user;

        // Adds both entities to the Entity Framework Core change tracker.
        await _dbContext.Users.AddAsync(
            user,
            cancellationToken);

        await _dbContext.EmailConfirmationTokens.AddAsync(
            confirmationToken,
            cancellationToken);

        // Persists both entities as a single atomic database operation.
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}