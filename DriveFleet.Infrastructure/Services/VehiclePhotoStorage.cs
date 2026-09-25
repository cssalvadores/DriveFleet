using DriveFleet.Application.Interfaces;

namespace DriveFleet.Infrastructure.Services;

/// <summary>
/// Stores vehicle photos in the API's local
/// wwwroot/uploads/vehicles directory.
/// </summary>
public class VehiclePhotoStorage : IVehiclePhotoStorage
{
    private const string UploadsDirectory = "uploads";
    private const string VehiclesDirectory = "vehicles";

    /// <summary>
    /// Saves a vehicle photo to local storage.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle that owns the photo.
    /// </param>
    /// <param name="photoStream">
    /// The stream containing the vehicle photo data.
    /// </param>
    /// <param name="fileExtension">
    /// The validated file extension, including the leading dot.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The relative path of the stored vehicle photo.
    /// </returns>
    public async Task<string> SaveAsync(
        int vehicleId,
        Stream photoStream,
        string fileExtension,
        CancellationToken cancellationToken = default)
    {
        var fileName =
            $"{vehicleId}_{Guid.NewGuid():N}{fileExtension}";

        var relativePath =
            $"/{UploadsDirectory}/{VehiclesDirectory}/{fileName}";

        var directoryPath =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                UploadsDirectory,
                VehiclesDirectory);

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
    /// Deletes a previously stored vehicle photo.
    /// </summary>
    /// <param name="relativePath">
    /// The relative path of the vehicle photo to delete.
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
