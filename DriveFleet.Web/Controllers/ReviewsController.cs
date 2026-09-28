using System.Net;
using DriveFleet.Web.Models.Reviews;
using DriveFleet.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Web.Controllers;

/// <summary>
/// Provides administrative Web pages
/// for vehicle review moderation.
/// </summary>
[Authorize(Roles = "Admin")]
public class ReviewsController : Controller
{
    private const string AccessTokenName =
        "access_token";

    private const string ReviewSuccessKey =
        "ReviewSuccess";

    private const string ReviewErrorKey =
        "ReviewError";

    private readonly AdminReviewApiClient
        _adminReviewApiClient;

    private readonly ILogger<ReviewsController>
        _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReviewsController"/> class.
    /// </summary>
    /// <param name="adminReviewApiClient">
    /// Client used to communicate with administrative
    /// review endpoints exposed by the DriveFleet API.
    /// </param>
    /// <param name="logger">
    /// Logger used to record unexpected communication errors.
    /// </param>
    public ReviewsController(
        AdminReviewApiClient adminReviewApiClient,
        ILogger<ReviewsController> logger)
    {
        _adminReviewApiClient =
            adminReviewApiClient;

        _logger =
            logger;
    }

    /// <summary>
    /// Displays all reviews available
    /// for administrative moderation.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request
    /// if the client disconnects.
    /// </param>
    /// <returns>
    /// The administrative review list page.
    /// </returns>
    [HttpGet("/admin/reviews")]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var accessToken =
            await GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(
            accessToken))
        {
            return await
                SignOutAndRedirectToLoginAsync();
        }

        try
        {
            var result =
                await _adminReviewApiClient.GetAllAsync(
                    accessToken,
                    cancellationToken);

            if (result.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                return await
                    SignOutAndRedirectToLoginAsync();
            }

            if (result.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (result.StatusCode !=
                HttpStatusCode.OK)
            {
                TempData[ReviewErrorKey] =
                    string.IsNullOrWhiteSpace(
                        result.Detail)
                        ? "The reviews could not be loaded."
                        : result.Detail;

                return View(
                    Array.Empty<AdminReviewViewModel>());
            }

            var viewModel =
                result.Reviews
                    .Select(MapReview)
                    .ToList();

            return View(
                viewModel);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while retrieving reviews.");

            TempData[ReviewErrorKey] =
                "The review service is temporarily unavailable.";

            return View(
                Array.Empty<AdminReviewViewModel>());
        }
    }

    /// <summary>
    /// Updates the public visibility
    /// of an existing review.
    /// </summary>
    /// <param name="reviewId">
    /// The identifier of the review to update.
    /// </param>
    /// <param name="isVisible">
    /// The requested public visibility state.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request
    /// if the client disconnects.
    /// </param>
    /// <returns>
    /// A redirect to the administrative review list.
    /// </returns>
    [HttpPost("/admin/reviews/{reviewId:int}/visibility")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetVisibility(
        int reviewId,
        bool isVisible,
        CancellationToken cancellationToken)
    {
        var accessToken =
            await GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(
            accessToken))
        {
            return await
                SignOutAndRedirectToLoginAsync();
        }

        try
        {
            var result =
                await _adminReviewApiClient
                    .SetVisibilityAsync(
                        accessToken,
                        reviewId,
                        isVisible,
                        cancellationToken);

            if (result.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                return await
                    SignOutAndRedirectToLoginAsync();
            }

            if (result.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (result.StatusCode ==
                HttpStatusCode.NotFound)
            {
                TempData[ReviewErrorKey] =
                    "The selected review could not be found.";

                return RedirectToAction(
                    nameof(Index));
            }

            if (result.StatusCode !=
                HttpStatusCode.OK)
            {
                TempData[ReviewErrorKey] =
                    string.IsNullOrWhiteSpace(
                        result.Detail)
                        ? "The review visibility could not be updated."
                        : result.Detail;

                return RedirectToAction(
                    nameof(Index));
            }

            TempData[ReviewSuccessKey] =
                isVisible
                    ? "The review is now publicly visible."
                    : "The review has been hidden from the public catalog.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while updating review {ReviewId} visibility.",
                reviewId);

            TempData[ReviewErrorKey] =
                "The review service is temporarily unavailable.";

            return RedirectToAction(
                nameof(Index));
        }
    }

    /// <summary>
    /// Permanently deletes an existing review.
    /// </summary>
    /// <param name="reviewId">
    /// The identifier of the review to delete.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request
    /// if the client disconnects.
    /// </param>
    /// <returns>
    /// A redirect to the administrative review list.
    /// </returns>
    [HttpPost("/admin/reviews/{reviewId:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int reviewId,
        CancellationToken cancellationToken)
    {
        var accessToken =
            await GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(
            accessToken))
        {
            return await
                SignOutAndRedirectToLoginAsync();
        }

        try
        {
            var result =
                await _adminReviewApiClient.DeleteAsync(
                    accessToken,
                    reviewId,
                    cancellationToken);

            if (result.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                return await
                    SignOutAndRedirectToLoginAsync();
            }

            if (result.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (result.StatusCode ==
                HttpStatusCode.NotFound)
            {
                TempData[ReviewErrorKey] =
                    "The selected review could not be found.";

                return RedirectToAction(
                    nameof(Index));
            }

            if (result.StatusCode !=
                HttpStatusCode.NoContent)
            {
                TempData[ReviewErrorKey] =
                    string.IsNullOrWhiteSpace(
                        result.Detail)
                        ? "The review could not be deleted."
                        : result.Detail;

                return RedirectToAction(
                    nameof(Index));
            }

            TempData[ReviewSuccessKey] =
                "The review was deleted successfully.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while deleting review {ReviewId}.",
                reviewId);

            TempData[ReviewErrorKey] =
                "The review service is temporarily unavailable.";

            return RedirectToAction(
                nameof(Index));
        }
    }

    /// <summary>
    /// Maps an API review model to the Web view model.
    /// </summary>
    /// <param name="review">
    /// The review returned by the DriveFleet API.
    /// </param>
    /// <returns>
    /// The review prepared for the administrative view.
    /// </returns>
    private static AdminReviewViewModel MapReview(
        AdminReviewApiModel review)
    {
        return new AdminReviewViewModel
        {
            ReviewId =
                review.ReviewId,

            ReservationVehicleId =
                review.ReservationVehicleId,

            VehicleId =
                review.VehicleId,

            Stars =
                review.Stars,

            Comment =
                review.Comment,

            IsVisible =
                review.IsVisible,

            CreatedAt =
                review.CreatedAt
        };
    }

    /// <summary>
    /// Gets the JWT access token stored
    /// in the authentication session.
    /// </summary>
    /// <returns>
    /// The access token when available;
    /// otherwise, null.
    /// </returns>
    private Task<string?> GetAccessTokenAsync()
    {
        return HttpContext.GetTokenAsync(
            AccessTokenName);
    }

    /// <summary>
    /// Signs out the current Web session
    /// and redirects to the login page.
    /// </summary>
    /// <returns>
    /// A redirect to the account login page.
    /// </returns>
    private async Task<IActionResult>
        SignOutAndRedirectToLoginAsync()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme);

        return RedirectToAction(
            "Login",
            "Account");
    }
}
