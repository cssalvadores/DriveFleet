using DriveFleet.Application.DTOs.Reviews;
using DriveFleet.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Api.Controllers;

/// <summary>
/// Provides publicly visible reviews for vehicles.
/// </summary>
[ApiController]
[Route("api/vehicles/{vehicleId:int}/reviews")]
public class VehicleReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VehicleReviewsController"/> class.
    /// </summary>
    public VehicleReviewsController(
        IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Gets publicly visible reviews for a vehicle.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ReviewResponse>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<IReadOnlyList<ReviewResponse>>> GetVisible(
            int vehicleId,
            CancellationToken cancellationToken)
    {
        var reviews =
            await _reviewService
                .GetVisibleByVehicleIdAsync(
                    vehicleId,
                    cancellationToken);

        return Ok(
            reviews);
    }
}