using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Entities;
using DriveFleet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriveFleet.Infrastructure.Repositories;

/// <summary>
/// Provides user persistence operations using Entity Framework Core.
/// This repository is responsible for accessing and modifying user data
/// through the DriveFleet database context.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly DriveFleetDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="dbContext">
    /// The database context used to access user data.
    /// </param>
    public UserRepository(DriveFleetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Checks whether a user with the specified email already exists.
    /// </summary>
    /// <param name="email">
    /// The email address to search for.
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
        // Uses an existence check instead of loading the complete user entity.
        return await _dbContext.Users
            .AnyAsync(
                user => user.Email == email,
                cancellationToken);
    }

    /// <summary>
    /// Adds a new user and persists the change to the database.
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

        // Persists all tracked changes to the database.
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
