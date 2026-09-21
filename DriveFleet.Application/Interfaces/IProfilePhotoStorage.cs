namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines storage operations for user profile photos.
/// </summary>
public interface IProfilePhotoStorage
{
    /// <summary>
    /// Saves a profile photo and returns the relative path
    /// that can be stored with the user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user who owns the photo.
    /// </param>
    /// <param name="photoStream">
    /// The stream containing the photo data.
    /// </param>
    /// <param name="fileExtension">
    /// The validated file extension, including the leading dot.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The relative path of the stored profile photo.
    /// </returns>
    Task<string> SaveAsync(
        int userId,
        Stream photoStream,
        string fileExtension,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a previously stored profile photo.
    /// </summary>
    /// <param name="relativePath">
    /// The relative path of the profile photo to delete.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    Task DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken = default);
}
