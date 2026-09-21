using DriveFleet.Application.Interfaces;

namespace DriveFleet.Infrastructure.Services;

/// <summary>
/// Stores user profile photos in the API's local
/// wwwroot/uploads/profiles directory.
/// </summary>
public class ProfilePhotoStorage : IProfilePhotoStorage
{
    private const string UploadsDirectory = "uploads";
    private const string ProfilesDirectory = "profiles";

    /// <summary>
    /// Saves a user profile photo to local storage.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user who owns the photo.
    /// </param>
    /// <param name="photoStream">
    /// The stream containing the profile photo data.
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
    public async Task<string> SaveAsync(
        int userId,
        Stream photoStream,
        string fileExtension,
        CancellationToken cancellationToken = default)
    {
        var fileName =
            $"{userId}_{Guid.NewGuid():N}{fileExtension}";

        var relativePath =
            $"/{UploadsDirectory}/{ProfilesDirectory}/{fileName}";

        var directoryPath =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                UploadsDirectory,
                ProfilesDirectory);

        Directory.CreateDirectory(
            directoryPath);

        var physicalPath =
            Path.Combine(
                directoryPath,
                fileName);

        await using var fileStream =
            new FileStream(
                physicalPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

        await photoStream.CopyToAsync(
            fileStream,
            cancellationToken);

        return relativePath;
    }

    /// <summary>
    /// Deletes a previously stored user profile photo.
    /// </summary>
    /// <param name="relativePath">
    /// The relative path of the profile photo to delete.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    public Task DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        var normalizedPath =
            relativePath
                .TrimStart('/')
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar);

        var physicalPath =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                normalizedPath);

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }
}
