using DriveFleet.Domain.Entities;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines persistence operations required during user registration.
/// </summary>
public interface IRegistrationRepository
{
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
    Task AddUserWithConfirmationTokenAsync(
        User user,
        EmailConfirmationToken confirmationToken,
        CancellationToken cancellationToken = default);
}
