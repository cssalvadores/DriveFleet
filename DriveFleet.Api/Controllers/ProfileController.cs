using System.Security.Claims;
using DriveFleet.Application.DTOs.Profile;
using DriveFleet.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Api.Controllers;

/// <summary>
/// Provides endpoints related to the authenticated user's profile.
/// </summary>
[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ProfileController"/> class.
    /// </summary>
    /// <param name="profileService">
    /// Service used to retrieve profile information.
    /// </param>
    public ProfileController(
        IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// Retrieves the profile of the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The authenticated user's profile.
    /// </returns>
    [HttpGet("me")]
    [ProducesResponseType(
        typeof(UserProfileResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponse>> GetMyProfile(
        CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(
                userIdValue,
                out var userId))
        {
            return Unauthorized();
        }

        var profile =
            await _profileService.GetProfileAsync(
                userId,
                cancellationToken);

        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    /// <summary>
    /// Updates the profile of the authenticated user.
    /// </summary>
    /// <param name="request">
    /// The profile information to update.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// No content when the profile is updated successfully.
    /// </returns>
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfile(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(
                userIdValue,
                out var userId))
        {
            return Unauthorized();
        }

        var updated =
            await _profileService.UpdateProfileAsync(
                userId,
                request,
                cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Updates the profile photo of the authenticated user.
    /// </summary>
    /// <param name="photo">
    /// The profile photo uploaded by the user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The relative path of the stored profile photo.
    /// </returns>
    [HttpPost("me/photo")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProfilePhotoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfilePhotoResponse>> UpdateMyProfilePhoto(
        IFormFile photo,
        CancellationToken cancellationToken)
    {
        const long maxFileSize = 5 * 1024 * 1024;

        if (photo is null || photo.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid profile photo",
                Detail = "Please select a profile photo.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (photo.Length > maxFileSize)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid profile photo",
                Detail = "The profile photo must not exceed 5 MB.",
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
                Title = "Invalid profile photo",
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
                Title = "Invalid profile photo",
                Detail = "The uploaded file is not a supported image type.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(
                userIdValue,
                out var userId))
        {
            return Unauthorized();
        }

        await using var photoStream =
            photo.OpenReadStream();

        var photoPath =
            await _profileService.UpdateProfilePhotoAsync(
                userId,
                photoStream,
                extension,
                cancellationToken);

        if (photoPath is null)
        {
            return NotFound();
        }

        return Ok(new ProfilePhotoResponse
        {
            Photo = photoPath
        });
    }
}

/// <summary>
/// Represents the result of a successful profile photo upload.
/// </summary>
public class ProfilePhotoResponse
{
    /// <summary>
    /// Gets or sets the relative path of the stored profile photo.
    /// </summary>
    public string Photo { get; set; } = string.Empty;
}
