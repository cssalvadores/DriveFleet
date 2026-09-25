using DriveFleet.Application.DTOs.Vehicles;
using DriveFleet.Application.Exceptions;
using DriveFleet.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Api.Controllers;

/// <summary>
/// Provides endpoints for retrieving vehicle information.
/// </summary>
[ApiController]
[Route("api/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VehiclesController"/> class.
    /// </summary>
    /// <param name="vehicleService">
    /// Service used to perform vehicle operations.
    /// </param>
    public VehiclesController(
        IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    /// <summary>
    /// Returns all vehicles.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The current vehicle catalogue.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<VehicleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VehicleResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var vehicles =
            await _vehicleService.GetAllAsync(cancellationToken);

        return Ok(vehicles);
    }

    /// <summary>
    /// Returns a vehicle by its identifier.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The requested vehicle when it exists.
    /// </returns>
    [HttpGet("{vehicleId:int}")]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleResponse>> GetById(
        int vehicleId,
        CancellationToken cancellationToken)
    {
        var vehicle =
            await _vehicleService.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null)
        {
            return NotFound();
        }

        return Ok(vehicle);
    }

    /// <summary>
    /// Retrieves all available vehicle categories.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The list of available vehicle categories.
    /// </returns>
    [HttpGet("categories")]
    [ProducesResponseType(
        typeof(List<VehicleOptionResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VehicleOptionResponse>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var categories =
            await _vehicleService.GetCategoriesAsync(
                cancellationToken);

        return Ok(categories);
    }

    /// <summary>
    /// Returns all available vehicle statuses.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The available vehicle statuses.
    /// </returns>
    [HttpGet("statuses")]
    [ProducesResponseType(
        typeof(List<VehicleOptionResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VehicleOptionResponse>>> GetVehicleStatuses(
        CancellationToken cancellationToken)
    {
        var statuses =
            await _vehicleService.GetVehicleStatusesAsync(
                cancellationToken);

        return Ok(statuses);
    }

    /// <summary>
    /// Creates a new vehicle.
    /// </summary>
    /// <param name="request">
    /// The vehicle information to create.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The newly created vehicle.
    /// </returns>
    [Authorize(Roles = "Admin,Employee")]
    [HttpPost]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehicleResponse>> Create(
        CreateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var vehicle =
                await _vehicleService.CreateAsync(
                    request,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    vehicleId = vehicle.VehicleId
                },
                vehicle);
        }
        catch (ConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Vehicle conflict",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (InvalidVehicleReferenceException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid vehicle reference",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Updates an existing vehicle.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle to update.
    /// </param>
    /// <param name="request">
    /// The updated vehicle information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// No content when the vehicle is updated successfully.
    /// </returns>
    [Authorize(Roles = "Admin,Employee")]
    [HttpPut("{vehicleId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int vehicleId,
        UpdateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated =
                await _vehicleService.UpdateAsync(
                    vehicleId,
                    request,
                    cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Vehicle conflict",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (InvalidVehicleReferenceException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid vehicle reference",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Creates or replaces a vehicle catalog photo
    /// in the specified display position.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle that owns the photo.
    /// </param>
    /// <param name="displayOrder">
    /// The catalog position of the photo, from 1 to 3.
    /// Position 1 represents the main catalog photo.
    /// </param>
    /// <param name="photo">
    /// The catalog photo uploaded for the vehicle.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The saved vehicle catalog photo.
    /// </returns>
    [Authorize(Roles = "Admin,Employee")]
    [HttpPut("{vehicleId:int}/photos/{displayOrder:int}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        typeof(VehiclePhotoResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehiclePhotoResponse>> SetPhoto(
        int vehicleId,
        int displayOrder,
        IFormFile photo,
        CancellationToken cancellationToken)
    {
        const long maxFileSize = 5 * 1024 * 1024;

        if (displayOrder is < 1 or > 3)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid vehicle photo position",
                Detail = "The display order must be between 1 and 3.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (photo is null || photo.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid vehicle photo",
                Detail = "Please select a vehicle photo.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (photo.Length > maxFileSize)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid vehicle photo",
                Detail = "The vehicle photo must not exceed 5 MB.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var extension =
            Path.GetExtension(photo.FileName)
                .ToLowerInvariant();

        var allowedExtensions =
            new HashSet<string>
            {
            ".jpg",
            ".jpeg",
            ".png"
            };

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid vehicle photo",
                Detail = "Only JPG, JPEG and PNG images are allowed.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var allowedContentTypes =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
            "image/jpeg",
            "image/png"
            };

        if (!allowedContentTypes.Contains(photo.ContentType))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid vehicle photo",
                Detail = "The uploaded file is not a supported image type.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        await using var photoStream =
            photo.OpenReadStream();

        var savedPhoto =
            await _vehicleService.SetPhotoAsync(
                vehicleId,
                displayOrder,
                photoStream,
                extension,
                cancellationToken);

        if (savedPhoto is null)
        {
            return NotFound();
        }

        return Ok(savedPhoto);
    } 

    /// <summary>
    /// Marks an existing vehicle as unavailable without deleting it.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle to mark as unavailable.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// No content when the vehicle is marked as unavailable successfully.
    /// </returns>
    [Authorize(Roles = "Admin,Employee")]
    [HttpPatch("{vehicleId:int}/unavailable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkUnavailable(
        int vehicleId,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated =
                await _vehicleService.MarkUnavailableAsync(
                    vehicleId,
                    cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidVehicleReferenceException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid vehicle reference",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
