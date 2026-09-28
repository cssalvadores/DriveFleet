using DriveFleet.Application.DTOs.Reviews;
using DriveFleet.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Api.Controllers;

/// <summary>
/// Provides administrative review moderation operations.
/// </summary>
[ApiController]
[Route("api/reviews")]
[Authorize(Roles = "Admin")]
public class AdminReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AdminReviewsController"/> class.
    /// </summary>
    /// <param name="reviewService">
    /// Service used to manage vehicle reviews.
    /// </param>
    public AdminReviewsController(
        IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Gets all reviews for administrative moderation.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// All reviews available for moderation.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ReviewResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    public async Task<
        ActionResult<IReadOnlyList<ReviewResponse>>> GetAll(
            CancellationToken cancellationToken)
    {
        var reviews =
            await _reviewService.GetAllAsync(
                cancellationToken);

        return Ok(
            reviews);
    }

    /// <summary>
    /// Updates the public visibility of a review.
    /// </summary>
    /// <param name="reviewId">
    /// The review identifier.
    /// </param>
    /// <param name="request">
    /// The requested visibility state.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The updated review.
    /// </returns>
    [HttpPatch("{reviewId:int}/visibility")]
    [ProducesResponseType(
        typeof(ReviewResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponse>>
        SetVisibility(
            int reviewId,
            UpdateReviewVisibilityRequest request,
            CancellationToken cancellationToken)
    {
        var review =
            await _reviewService.SetVisibilityAsync(
                reviewId,
                request.IsVisible,
                cancellationToken);

        if (review is null)
        {
            return NotFound();
        }

        return Ok(
            review);
    }

    /// <summary>
    /// Permanently deletes a review.
    /// </summary>
    /// <param name="reviewId">
    /// The review identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// No content when the review is deleted.
    /// </returns>
    [HttpDelete("{reviewId:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int reviewId,
        CancellationToken cancellationToken)
    {
        var deleted =
            await _reviewService.DeleteAsync(
                reviewId,
                cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
