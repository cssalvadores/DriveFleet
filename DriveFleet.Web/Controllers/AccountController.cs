using System.Net;
using DriveFleet.Web.Models.Auth;
using DriveFleet.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Web.Controllers;

/// <summary>
/// Provides account-related pages for the DriveFleet web application.
/// </summary>
public class AccountController : Controller
{
    private readonly AuthApiClient _authApiClient;
    private readonly ILogger<AccountController> _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AccountController"/> class.
    /// </summary>
    /// <param name="authApiClient">
    /// Client used to communicate with authentication endpoints
    /// exposed by the DriveFleet API.
    /// </param>
    /// <param name="logger">
    /// Logger used to record unexpected communication errors.
    /// </param>
    public AccountController(
        AuthApiClient authApiClient,
        ILogger<AccountController> logger)
    {
        _authApiClient = authApiClient;
        _logger = logger;
    }

    /// <summary>
    /// Processes an email confirmation link.
    /// </summary>
    /// <param name="token">
    /// The raw email confirmation token received through the query string.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The email confirmation result page.
    /// </returns>
    [HttpGet("/account/confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string? token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View(new ConfirmEmailViewModel
            {
                State = ConfirmEmailViewState.Error,
                Message = "The email confirmation link is invalid."
            });
        }

        try
        {
            var result = await _authApiClient.ConfirmEmailAsync(
                token,
                cancellationToken);

            if (result.StatusCode == HttpStatusCode.NoContent)
            {
                return View(new ConfirmEmailViewModel
                {
                    State = ConfirmEmailViewState.Success,
                    Message =
                        "Your email address has been confirmed successfully."
                });
            }

            if (result.StatusCode == HttpStatusCode.Conflict)
            {
                return View(new ConfirmEmailViewModel
                {
                    State = ConfirmEmailViewState.AlreadyConfirmed,
                    Message =
                        "Your email address has already been confirmed."
                });
            }

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                return View(new ConfirmEmailViewModel
                {
                    State = ConfirmEmailViewState.Error,
                    Message = string.IsNullOrWhiteSpace(result.Detail)
                        ? "The email confirmation link is invalid or has expired."
                        : result.Detail
                });
            }

            return View(new ConfirmEmailViewModel
            {
                State = ConfirmEmailViewState.Error,
                Message =
                    "We could not confirm your email address. Please try again."
            });
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API during email confirmation.");

            return View(new ConfirmEmailViewModel
            {
                State = ConfirmEmailViewState.Error,
                Message =
                    "The confirmation service is temporarily unavailable."
            });
        }
    }
}
