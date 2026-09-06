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
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
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
}
