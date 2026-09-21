using DriveFleet.Domain.Entities;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines persistence operations related to users.
/// This interface allows the Application layer to work with users
/// without depending directly on Entity Framework Core or SQL Server.
/// </summary>
public interface IUserRepository
{
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
    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default);

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
    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user and persists it to the data store.
    /// </summary>
    /// <param name="user">
    /// The user entity to be added.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    Task AddAsync(
        User user,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by identifier.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    /// <returns>
    /// The matching user, or null if no user exists with the specified identifier.
    /// </returns>
    Task<User?> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists pending user changes to the data store.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);


}
