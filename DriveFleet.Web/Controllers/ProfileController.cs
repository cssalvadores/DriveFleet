using System.Net;
using DriveFleet.Web.Models.Profile;
using DriveFleet.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Web.Controllers;

/// <summary>
/// Provides profile-related pages for the authenticated user.
/// </summary>
[Authorize]
public class ProfileController : Controller
{
    private readonly ProfileApiClient _profileApiClient;
    private readonly ILogger<ProfileController> _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ProfileController"/> class.
    /// </summary>
    /// <param name="profileApiClient">
    /// Client used to communicate with profile endpoints
    /// exposed by the DriveFleet API.
    /// </param>
    /// <param name="logger">
    /// Logger used to record unexpected communication errors.
    /// </param>
    public ProfileController(
        ProfileApiClient profileApiClient,
        ILogger<ProfileController> logger)
    {
        _profileApiClient = profileApiClient;
        _logger = logger;
    }

    /// <summary>
    /// Displays the authenticated user's profile.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The profile page.
    /// </returns>
    [HttpGet("/profile")]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var accessToken =
            await HttpContext.GetTokenAsync(
                "access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(
                "Login",
                "Account");
        }

        try
        {
            var result =
                await _profileApiClient.GetProfileAsync(
                    accessToken,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (result.StatusCode != HttpStatusCode.OK ||
                result.UserId is null ||
                string.IsNullOrWhiteSpace(result.FirstName) ||
                string.IsNullOrWhiteSpace(result.Email) ||
                string.IsNullOrWhiteSpace(result.Role) ||
                result.EmailConfirmed is null)
            {
                return View("ProfileError");
            }

            var model = new UserProfileViewModel
            {
                UserId = result.UserId.Value,
                FirstName = result.FirstName,
                LastName = result.LastName,
                Email = result.Email,
                Phone = result.Phone,
                Photo = result.Photo,
                Provider = result.Provider,
                EmailConfirmed = result.EmailConfirmed.Value,
                Role = result.Role
            };

            return View(model);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while retrieving the user profile.");

            return View("ProfileError");
        }
    }

    /// <summary>
    /// Displays the profile editing form
    /// for the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The profile editing page.
    /// </returns>
    [HttpGet("/profile/edit")]
    public async Task<IActionResult> Edit(
        CancellationToken cancellationToken)
    {
        var accessToken =
            await HttpContext.GetTokenAsync(
                "access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(
                "Login",
                "Account");
        }

        try
        {
            var result =
                await _profileApiClient.GetProfileAsync(
                    accessToken,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (result.StatusCode != HttpStatusCode.OK ||
                string.IsNullOrWhiteSpace(result.FirstName))
            {
                return View("ProfileError");
            }

            var model = new EditProfileViewModel
            {
                FirstName = result.FirstName,
                LastName = result.LastName,
                Phone = result.Phone
            };

            return View(model);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while preparing the profile editing form.");

            return View("ProfileError");
        }
    }    

    /// <summary>
    /// Processes the profile editing form
    /// for the authenticated user.
    /// </summary>
    /// <param name="model">
    /// The profile information entered by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// A redirect to the profile page when the update succeeds,
    /// or the editing page when it fails.
    /// </returns>
    [HttpPost("/profile/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EditProfileViewModel model,
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
                "Login",
                "Account");
        }

        try
        {
            var result =
                await _profileApiClient.UpdateProfileAsync(
                    accessToken,
                    model.FirstName,
                    model.LastName,
                    model.Phone,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.NoContent)
            {
                return RedirectToAction(
                    nameof(Index));
            }

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(result.Detail)
                        ? "The profile information is invalid."
                        : result.Detail);

                return View(model);
            }

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            ModelState.AddModelError(
                string.Empty,
                "We could not update your profile. Please try again.");

            return View(model);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while updating the user profile.");

            ModelState.AddModelError(
                string.Empty,
                "The profile service is temporarily unavailable.");

            return View(model);
        }
    }

    /// <summary>
    /// Displays the profile photo upload form
    /// for the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The profile photo page.
    /// </returns>
    [HttpGet("/profile/photo")]
    public async Task<IActionResult> Photo(
        CancellationToken cancellationToken)
    {
        var accessToken =
            await HttpContext.GetTokenAsync(
                "access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(
                "Login",
                "Account");
        }

        try
        {
            var result =
                await _profileApiClient.GetProfileAsync(
                    accessToken,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return View("ProfileError");
            }

            var model = new ProfilePhotoViewModel
            {
                CurrentPhoto = result.Photo
            };

            return View(model);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while preparing the profile photo form.");

            return View("ProfileError");
        }
    }

    /// <summary>
    /// Processes a profile photo upload
    /// for the authenticated user.
    /// </summary>
    /// <param name="model">
    /// The profile photo selected by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// A redirect to the profile page when the upload succeeds,
    /// or the upload page when it fails.
    /// </returns>
    [HttpPost("/profile/photo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Photo(
        ProfilePhotoViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Photo is null)
        {
            ModelState.AddModelError(
                nameof(model.Photo),
                "Please select a profile photo.");

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
                "Login",
                "Account");
        }

        try
        {
            await using var photoStream =
                model.Photo.OpenReadStream();

            var result =
                await _profileApiClient.UploadProfilePhotoAsync(
                    accessToken,
                    photoStream,
                    model.Photo.FileName,
                    model.Photo.ContentType,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.OK &&
                !string.IsNullOrWhiteSpace(result.Photo))
            {
                return RedirectToAction(
                    nameof(Index));
            }

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(result.Detail)
                        ? "The selected profile photo is invalid."
                        : result.Detail);

                return View(model);
            }

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            ModelState.AddModelError(
                string.Empty,
                "We could not update your profile photo. Please try again.");

            return View(model);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while uploading the profile photo.");

            ModelState.AddModelError(
                string.Empty,
                "The profile photo service is temporarily unavailable.");

            return View(model);
        }
    }

}
