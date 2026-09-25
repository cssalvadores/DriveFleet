namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines storage operations for vehicle photos.
/// </summary>
public interface IVehiclePhotoStorage
{
    /// <summary>
    /// Saves a vehicle photo and returns the relative path
    /// that can be stored with the vehicle.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle that owns the photo.
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
    /// The relative path of the stored vehicle photo.
    /// </returns>
    Task<string> SaveAsync(
        int vehicleId,
        Stream photoStream,
        string fileExtension,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a previously stored vehicle photo.
    /// </summary>
    /// <param name="relativePath">
    /// The relative path of the vehicle photo to delete.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    Task DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken = default);
}
