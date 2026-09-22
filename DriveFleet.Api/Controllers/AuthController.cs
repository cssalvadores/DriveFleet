using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DriveFleet.Application.DTOs.Auth;
using DriveFleet.Application.Exceptions;
using DriveFleet.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Api.Controllers;

/// <summary>
/// Provides endpoints for authentication-related operations.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">
    /// Service responsible for authentication-related application operations.
    /// </param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new client account.
    /// </summary>
    /// <param name="request">
    /// The registration data provided by the client.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// Information about the newly registered user.
    /// </returns>
    [HttpPost("register")]
    [ProducesResponseType(
        typeof(RegisterResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RegisterAsync(
                request,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                response);
        }
        catch (ConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Registration conflict",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    /// <summary>
    /// Confirms a user's email address using a confirmation token.
    /// </summary>
    /// <param name="request">
    /// The request containing the raw email confirmation token.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// A successful response when the email address has been confirmed.
    /// </returns>
    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmEmail(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _authService.ConfirmEmailAsync(
                request.Token,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidTokenException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid confirmation token",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (ConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Email confirmation conflict",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    /// <summary>
    /// Authenticates a user using email and password credentials.
    /// </summary>
    /// <param name="request">
    /// The credentials provided by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The authenticated user's information and JWT access token.
    /// </returns>
    [HttpPost("login")]
    [ProducesResponseType(
        typeof(LoginResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(
                request,
                cancellationToken);

            return Ok(response);
        }
        catch (InvalidCredentialsException exception)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication failed",
                Detail = exception.Message,
                Status = StatusCodes.Status401Unauthorized
            });
        }
        catch (EmailNotConfirmedException exception)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ProblemDetails
                {
                    Title = "Email confirmation required",
                    Detail = exception.Message,
                    Status = StatusCodes.Status403Forbidden
                });
        }
    }

    /// <summary>
    /// Starts the password recovery process for a user account.
    /// </summary>
    /// <param name="request">
    /// The email address associated with the account.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A generic response that does not reveal whether the email exists.
    /// </returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(
        typeof(ForgotPasswordResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _authService.ForgotPasswordAsync(
                request,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Resets a user's password using a valid password reset token.
    /// </summary>
    /// <param name="request">
    /// The reset token and new password information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// No content when the password has been reset successfully.
    /// </returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _authService.ResetPasswordAsync(
                request,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidTokenException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid password reset token",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Changes the password of the authenticated user.
    /// </summary>
    /// <param name="request">
    /// The current password and new password information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// No content when the password has been changed successfully.
    /// </returns>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await _authService.ChangePasswordAsync(
                userId,
                request,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidCurrentPasswordException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid current password",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized();
        }
    }

    /// <summary>
    /// Revokes the JWT access token of the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// No content when the access token has been revoked successfully.
    /// </returns>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        var jti =
            User.FindFirstValue(
                JwtRegisteredClaimNames.Jti);

        var expirationValue =
            User.FindFirstValue(
                JwtRegisteredClaimNames.Exp);

        if (string.IsNullOrWhiteSpace(jti) ||
            !long.TryParse(
                expirationValue,
                out var expirationUnixSeconds))
        {
            return Unauthorized();
        }

        var expiresAt =
            DateTimeOffset
                .FromUnixTimeSeconds(expirationUnixSeconds)
                .UtcDateTime;

        await _authService.RevokeTokenAsync(
            jti,
            expiresAt,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Returns identity information for the currently authenticated user.
    /// </summary>
    /// <returns>
    /// The authenticated user's identifier, email address and role.
    /// </returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(
        typeof(CurrentUserResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserResponse> Me()
    {
        // Reads the authenticated user's identifier from the JWT claims.
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        // Reads the authenticated user's email address from the JWT claims.
        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        // Reads the authenticated user's role from the JWT claims.
        var role =
            User.FindFirstValue(
                ClaimTypes.Role);

        if (!int.TryParse(
                userIdValue,
                out var userId) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized();
        }

        return Ok(new CurrentUserResponse
        {
            UserId = userId,
            Email = email,
            Role = role
        });
    }

}
