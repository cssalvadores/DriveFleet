//using DriveFleet.Web.Models.Account;
using System.Net;
using DriveFleet.Web.Models.Auth;
using DriveFleet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

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
    /// Authenticates a user and creates the web authentication cookie.
    /// </summary>
    /// <param name="model">
    /// The credentials entered by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// A redirect to the home page when authentication succeeds,
    /// or the login page when authentication fails.
    /// </returns>
    [HttpPost("/account/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _authApiClient.LoginAsync(
                model.Email,
                model.Password,
                cancellationToken);

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return View(model);
            }

            if (result.StatusCode != HttpStatusCode.OK ||
                result.UserId is null ||
                string.IsNullOrWhiteSpace(result.Email) ||
                string.IsNullOrWhiteSpace(result.Role) ||
                string.IsNullOrWhiteSpace(result.AccessToken) ||
                result.ExpiresAt is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "We could not sign you in. Please try again.");

                return View(model);
            }

            var fullName = string.Join(
                " ",
                new[]
                {
                result.FirstName,
                result.LastName
                }
                .Where(value => !string.IsNullOrWhiteSpace(value)));

            var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                result.UserId.Value.ToString()),

            new(
                ClaimTypes.Name,
                string.IsNullOrWhiteSpace(fullName)
                    ? result.Email
                    : fullName),

            new(
                ClaimTypes.Email,
                result.Email),

            new(
                ClaimTypes.Role,
                result.Role)
        };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var authenticationProperties =
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = new DateTimeOffset(
                        DateTime.SpecifyKind(
                            result.ExpiresAt.Value,
                            DateTimeKind.Utc))
                };

            authenticationProperties.StoreTokens(
            [
                new AuthenticationToken
            {
                Name = "access_token",
                Value = result.AccessToken
            }
            ]);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authenticationProperties);

            return RedirectToAction(
                "Index",
                "Home");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API during login.");

            ModelState.AddModelError(
                string.Empty,
                "The login service is temporarily unavailable.");

            return View(model);
        }
    }

    /// <summary>
    /// Displays the login form.
    /// </summary>
    /// <returns>
    /// The login page.
    /// </returns>
    [HttpGet("/account/login")]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(
                "Index",
                "Home");
        }

        return View(new LoginViewModel());
    }

    /// <summary>
    /// Signs out the currently authenticated web user.
    /// </summary>
    /// <returns>
    /// A redirect to the home page.
    /// </returns>
    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(
            "Index",
            "Home");
    }

    /// <summary>
    /// Displays the password change form
    /// for the authenticated user.
    /// </summary>
    /// <returns>
    /// The password change page.
    /// </returns>
    [Authorize]
    [HttpGet("/account/change-password")]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    /// <summary>
    /// Processes a password change request
    /// for the authenticated user.
    /// </summary>
    /// <param name="model">
    /// The password change information entered by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The password change result page.
    /// </returns>
    [Authorize]
    [HttpPost("/account/change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var accessToken =
            await HttpContext.GetTokenAsync(
                "access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(
                nameof(Login));
        }

        try
        {
            var result =
                await _authApiClient.ChangePasswordAsync(
                    accessToken,
                    model.CurrentPassword,
                    model.NewPassword,
                    model.ConfirmNewPassword,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.NoContent)
            {
                return View("ChangePasswordSuccess");
            }

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(result.Detail)
                        ? "The current password is invalid."
                        : result.Detail);

                return View(model);
            }

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    nameof(Login));
            }

            ModelState.AddModelError(
                string.Empty,
                "We could not change your password. Please try again.");

            return View(model);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API during password change.");

            ModelState.AddModelError(
                string.Empty,
                "The password change service is temporarily unavailable.");

            return View(model);
        }
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
