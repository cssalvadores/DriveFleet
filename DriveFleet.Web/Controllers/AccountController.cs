//using DriveFleet.Web.Models.Account;
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

    /// <summary>
    /// Displays the password reset form.
    /// </summary>
    /// <param name="token">
    /// The raw password reset token received through the query string.
    /// </param>
    /// <returns>
    /// The password reset page when the token is present.
    /// </returns>
    [HttpGet("/account/reset-password")]
    public IActionResult ResetPassword(
        [FromQuery] string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View("ResetPasswordInvalid");
        }

        return View(new ResetPasswordViewModel
        {
            Token = token
        });
    }

    /// <summary>
    /// Processes the password reset form.
    /// </summary>
    /// <param name="model">
    /// The password reset information entered by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The password reset result page.
    /// </returns>
    [HttpPost("/account/reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result =
                await _authApiClient.ResetPasswordAsync(
                    model.Token,
                    model.NewPassword,
                    model.ConfirmPassword,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.NoContent)
            {
                return View("ResetPasswordSuccess");
            }

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(result.Detail)
                        ? "The password reset link is invalid, expired or has already been used."
                        : result.Detail);

                return View(model);
            }

            ModelState.AddModelError(
                string.Empty,
                "We could not reset your password. Please try again.");

            return View(model);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API during password reset.");

            ModelState.AddModelError(
                string.Empty,
                "The password reset service is temporarily unavailable.");

            return View(model);
        }
    }

}
