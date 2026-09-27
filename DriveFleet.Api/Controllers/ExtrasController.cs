using DriveFleet.Application.DTOs.Extras;
using DriveFleet.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Api.Controllers;

/// <summary>
/// Provides endpoints for the DriveFleet extras catalog.
/// </summary>
[ApiController]
[Route("api/extras")]
public class ExtrasController : ControllerBase
{
    private readonly IExtraService _extraService;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ExtrasController"/> class.
    /// </summary>
    /// <param name="extraService">
    /// Service responsible for extra-related application operations.
    /// </param>
    public ExtrasController(
        IExtraService extraService)
    {
        _extraService = extraService;
    }

    /// <summary>
    /// Gets all active extras available in the catalog.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>The active extras catalog.</returns>
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ExtraResponse>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<IReadOnlyList<ExtraResponse>>> GetActive(
            CancellationToken cancellationToken)
    {
        var extras =
            await _extraService.GetActiveAsync(
                cancellationToken);

        return Ok(extras);
    }
}
