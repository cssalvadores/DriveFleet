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

    /// <summary>
    /// Confirms a user's email address using a confirmation token.
    /// </summary>
    /// <param name="token">
    /// The raw email confirmation token received from the client.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    Task ConfirmEmailAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user using email and password credentials.
    /// </summary>
    /// <param name="request">
    /// The credentials provided by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The authenticated user's information and JWT access token.
    /// </returns>
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
