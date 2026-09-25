using DriveFleet.Application.DTOs.Vehicles;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines application operations for vehicle management.
/// </summary>
public interface IVehicleService
{
    /// <summary>
    /// Returns all vehicles.
    /// </summary>
    Task<List<VehicleResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a vehicle by its identifier.
    /// </summary>
    Task<VehicleResponse?> GetByIdAsync(
        int vehicleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new vehicle.
    /// </summary>
    Task<VehicleResponse> CreateAsync(
        CreateVehicleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing vehicle.
    /// </summary>
    Task<bool> UpdateAsync(
        int vehicleId,
        UpdateVehicleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the available vehicle categories.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The list of vehicle category options.
    /// </returns>
    Task<List<VehicleOptionResponse>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the available vehicle statuses.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The list of vehicle status options.
    /// </returns>
    Task<List<VehicleOptionResponse>> GetVehicleStatusesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing vehicle as unavailable without deleting it.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle to mark as unavailable.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// True when the vehicle is updated successfully;
    /// otherwise, false when the vehicle does not exist.
    /// </returns>
    Task<bool> MarkUnavailableAsync(
        int vehicleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a catalog photo in the specified display position
    /// for an existing vehicle.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle that owns the photo.
    /// </param>
    /// <param name="displayOrder">
    /// The catalog position of the photo, from 1 to 3.
    /// Position 1 represents the main catalog photo.
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
    /// The saved vehicle photo, or null when the vehicle does not exist.
    /// </returns>
    Task<VehiclePhotoResponse?> SetPhotoAsync(
        int vehicleId,
        int displayOrder,
        Stream photoStream,
        string fileExtension,
        CancellationToken cancellationToken = default);
}