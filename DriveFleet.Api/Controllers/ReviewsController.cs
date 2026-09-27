using System.Security.Claims;
using DriveFleet.Application.DTOs.Reviews;
using DriveFleet.Application.Exceptions;
using DriveFleet.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Api.Controllers;

/// <summary>
/// Provides vehicle review operations.
/// </summary>
[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReviewsController"/> class.
    /// </summary>
    public ReviewsController(
        IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Gets the authenticated client's review
    /// for a reserved vehicle.
    /// </summary>
    [Authorize(Roles = "Client")]
    [HttpGet(
        "{reservationId:int}/vehicles/" +
        "{reservationVehicleId:int}/review")]
    [ProducesResponseType(
        typeof(ReviewResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponse>> Get(
        int reservationId,
        int reservationVehicleId,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(
            out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var review =
                await _reviewService
                    .GetByReservationVehicleAsync(
                        userId,
                        reservationId,
                        reservationVehicleId,
                        cancellationToken);

            if (review is null)
            {
                return NotFound();
            }

            return Ok(
                review);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Creates a review for a vehicle included
    /// in a completed reservation.
    /// </summary>
    /// <param name="reservationId">
    /// The reservation identifier.
    /// </param>
    /// <param name="reservationVehicleId">
    /// The reserved vehicle identifier.
    /// </param>
    /// <param name="request">
    /// The submitted rating and optional comment.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The created review.
    /// </returns>
    [Authorize(Roles = "Client")]
    [HttpPost(
        "{reservationId:int}/vehicles/" +
        "{reservationVehicleId:int}/review")]
    [ProducesResponseType(
        typeof(ReviewResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewResponse>> Create(
        int reservationId,
        int reservationVehicleId,
        CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(
            out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var review =
                await _reviewService.CreateAsync(
                    userId,
                    reservationId,
                    reservationVehicleId,
                    request,
                    cancellationToken);

            if (review is null)
            {
                return NotFound();
            }

            return StatusCode(
                StatusCodes.Status201Created,
                review);
        }
        catch (InvalidReviewException exception)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Title = "Invalid review",
                    Detail = exception.Message,
                    Status =
                        StatusCodes.Status400BadRequest
                });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ConflictException exception)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Review conflict",
                    Detail = exception.Message,
                    Status =
                        StatusCodes.Status409Conflict
                });
        }
    }

    /// <summary>
    /// Tries to obtain the authenticated user's identifier
    /// from the JWT claims.
    /// </summary>
    private bool TryGetAuthenticatedUserId(
        out int userId)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return int.TryParse(
            userIdValue,
            out userId);
    }
}
