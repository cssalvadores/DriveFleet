using DriveFleet.Application.DTOs.Auth;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines authentication-related application operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new client account.
    /// </summary>
    /// <param name="request">
    /// The registration data provided by the client.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// Information about the successfully registered user.
    /// </returns>
    Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);
}
