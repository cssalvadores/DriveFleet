using DriveFleet.Application.DTOs.Profile;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines application operations related to
/// the authenticated user's profile.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Retrieves the profile of the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the authenticated user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The user's profile, or null if the user cannot be found.
    /// </returns>
    Task<UserProfileResponse?> GetProfileAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the profile information of the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the authenticated user.
    /// </param>
    /// <param name="request">
    /// The profile information to update.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// True if the user was found and updated; otherwise, false.
    /// </returns>
    Task<bool> UpdateProfileAsync(
        int userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the profile photo of the specified user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the authenticated user.
    /// </param>
    /// <param name="photoStream">
    /// The stream containing the new profile photo.
    /// </param>
    /// <param name="fileExtension">
    /// The file extension of the uploaded photo.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The relative path of the new profile photo,
    /// or null if the user cannot be found.
    /// </returns>
    Task<string?> UpdateProfilePhotoAsync(
        int userId,
        Stream photoStream,
        string fileExtension,
        CancellationToken cancellationToken = default);
}
