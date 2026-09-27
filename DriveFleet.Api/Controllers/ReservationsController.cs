using System.Security.Claims;
using DriveFleet.Application.DTOs.Reservations;
using DriveFleet.Application.Exceptions;
using DriveFleet.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Api.Controllers;

/// <summary>
/// Provides endpoints for reservation management.
/// </summary>
[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReservationsController"/> class.
    /// </summary>
    /// <param name="reservationService">
    /// Service responsible for reservation application operations.
    /// </param>
    public ReservationsController(
        IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    /// <summary>
    /// Creates a reservation for the authenticated client.
    /// </summary>
    /// <param name="request">
    /// The vehicles, rental periods and extras selected by the client.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The newly created reservation.
    /// </returns>
    [Authorize(Roles = "Client")]
    [HttpPost]
    [ProducesResponseType(
        typeof(ReservationResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponse>> Create(
        CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var reservation =
                await _reservationService.CreateAsync(
                    userId,
                    request,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    reservationId =
                        reservation.ReservationId
                },
                reservation);
        }
        catch (InvalidReservationException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid reservation",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (ConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Reservation conflict",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    /// <summary>
    /// Starts a pending reservation.
    /// </summary>
    /// <param name="reservationId">
    /// The reservation identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// No content when the reservation is started.
    /// </returns>
    [Authorize(Roles = "Admin,Employee")]
    [HttpPatch("{reservationId:int}/start")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Start(
        int reservationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var started =
                await _reservationService.StartAsync(
                    reservationId,
                    cancellationToken);

            if (!started)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Reservation start conflict",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    /// <summary>
    /// Completes an active reservation.
    /// </summary>
    /// <param name="reservationId">
    /// The reservation identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// No content when the reservation is completed.
    /// </returns>
    [Authorize(Roles = "Admin,Employee")]
    [HttpPatch("{reservationId:int}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(
        int reservationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var completed =
                await _reservationService.CompleteAsync(
                    reservationId,
                    cancellationToken);

            if (!completed)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Reservation completion conflict",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    /// <summary>
    /// Gets reservations belonging to the authenticated user
    /// that match the supplied filters.
    /// </summary>
    /// <param name="filter">
    /// The optional reservation filtering criteria.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The authenticated user's matching reservations.
    /// </returns>
    [HttpGet("mine")]
    [ProducesResponseType(
        typeof(IReadOnlyList<ReservationResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<
        ActionResult<IReadOnlyList<ReservationResponse>>> GetMine(
            [FromQuery] ReservationFilterRequest filter,
            CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var reservations =
                await _reservationService.GetByUserIdAsync(
                    userId,
                    filter,
                    cancellationToken);

            return Ok(reservations);
        }
        catch (InvalidReservationFilterException exception)
        {
            return BadRequest(
                CreateInvalidFilterProblemDetails(
                    exception));
        }
    }

    /// <summary>
    /// Gets reservations for authorized staff
    /// that match the supplied filters.
    /// </summary>
    /// <param name="filter">
    /// The optional reservation filtering criteria.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The matching reservations.
    /// </returns>
    [Authorize(Roles = "Admin,Employee")]
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ReservationResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<
        ActionResult<IReadOnlyList<ReservationResponse>>> GetAll(
            [FromQuery] ReservationFilterRequest filter,
            CancellationToken cancellationToken)
    {
        try
        {
            var reservations =
                await _reservationService.GetAllAsync(
                    filter,
                    cancellationToken);

            return Ok(reservations);
        }
        catch (InvalidReservationFilterException exception)
        {
            return BadRequest(
                CreateInvalidFilterProblemDetails(
                    exception));
        }
    }

    /// <summary>
    /// Gets the details of a reservation.
    /// </summary>
    /// <param name="reservationId">
    /// The identifier of the reservation to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The requested reservation.
    /// </returns>
    [HttpGet("{reservationId:int}")]
    [ProducesResponseType(
        typeof(ReservationResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponse>> GetById(
        int reservationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized();
        }

        var reservation =
            await _reservationService.GetByIdAsync(
                reservationId,
                cancellationToken);

        if (reservation is null)
        {
            return NotFound();
        }

        if (!CanManageAllReservations() &&
            reservation.UserId != userId)
        {
            return Forbid();
        }

        return Ok(reservation);
    }

    /// <summary>
    /// Cancels an existing reservation.
    /// </summary>
    /// <param name="reservationId">
    /// The identifier of the reservation to cancel.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// No content when the reservation is cancelled.
    /// </returns>
    [HttpPatch("{reservationId:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        int reservationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var cancelled =
                await _reservationService.CancelAsync(
                    reservationId,
                    userId,
                    CanManageAllReservations(),
                    cancellationToken);

            if (!cancelled)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title =
                    "Reservation cancellation conflict",

                Detail =
                    exception.Message,

                Status =
                    StatusCodes.Status409Conflict
            });
        }
    }

    /// <summary>
    /// Creates the problem details returned when reservation
    /// filtering criteria are invalid.
    /// </summary>
    /// <param name="exception">
    /// The reservation filter validation exception.
    /// </param>
    /// <returns>
    /// Problem details describing the invalid filter.
    /// </returns>
    private static ProblemDetails
        CreateInvalidFilterProblemDetails(
            InvalidReservationFilterException exception)
    {
        return new ProblemDetails
        {
            Title = "Invalid reservation filter",
            Detail = exception.Message,
            Status = StatusCodes.Status400BadRequest
        };
    }

    /// <summary>
    /// Tries to obtain the authenticated user's identifier
    /// from the JWT claims.
    /// </summary>
    /// <param name="userId">
    /// The authenticated user identifier when available.
    /// </param>
    /// <returns>
    /// True when a valid user identifier is available;
    /// otherwise, false.
    /// </returns>
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

    /// <summary>
    /// Determines whether the authenticated user may manage
    /// reservations belonging to other users.
    /// </summary>
    /// <returns>
    /// True for administrators and employees;
    /// otherwise, false.
    /// </returns>
    private bool CanManageAllReservations()
    {
        return User.IsInRole("Admin") ||
               User.IsInRole("Employee");
    }
}
