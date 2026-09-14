using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Entities;
using DriveFleet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriveFleet.Infrastructure.Repositories;

/// <summary>
/// Provides user persistence and query operations
/// using Entity Framework Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly DriveFleetDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="dbContext">
    /// The database context used to access user data.
    /// </param>
    public UserRepository(
        DriveFleetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Determines whether a user with the specified email already exists.
    /// </summary>
    /// <param name="email">
    /// The normalized email address to check.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    /// <returns>
    /// True if a user with the specified email exists; otherwise, false.
    /// </returns>
    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AnyAsync(
            user => user.Email == email,
            cancellationToken);
    }

    /// <summary>
    /// Retrieves a user by email address.
    /// </summary>
    /// <param name="email">
    /// The normalized email address of the user to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    /// <returns>
    /// The matching user, or null if no user exists with the specified email.
    /// </returns>
    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.Email == email,
                cancellationToken);
    }

    /// <summary>
    /// Adds a new user and persists it to the database.
    /// </summary>
    /// <param name="user">
    /// The user entity to be added.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        // Adds the user entity to the Entity Framework Core change tracker.
        await _dbContext.Users.AddAsync(
            user,
            cancellationToken);

        // Persists the new user to the database.
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
