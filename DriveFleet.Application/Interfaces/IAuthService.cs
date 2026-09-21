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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ForgotPasswordResponse> ForgotPasswordAsync(
    ForgotPasswordRequest request,
    CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a user's password using a valid password reset token.
    /// </summary>
    /// <param name="request">
    /// The reset token and new password information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    Task ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the password of an authenticated user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the authenticated user.
    /// </param>
    /// <param name="request">
    /// The current password and new password information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    Task ChangePasswordAsync(
        int userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
}
