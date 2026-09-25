using DriveFleet.Domain.Entities;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines persistence operations for vehicles.
/// </summary>
public interface IVehicleRepository
{
    /// <summary>
    /// Returns all vehicles with their category and current status.
    /// </summary>
    Task<List<Vehicle>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a vehicle by its identifier.
    /// </summary>
    Task<Vehicle?> GetByIdAsync(
        int vehicleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a license plate is already registered.
    /// </summary>
    Task<bool> LicensePlateExistsAsync(
        string licensePlate,
        int? excludeVehicleId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a vehicle category exists.
    /// </summary>
    Task<bool> CategoryExistsAsync(
        int categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a vehicle status exists.
    /// </summary>
    Task<bool> VehicleStatusExistsAsync(
        int vehicleStatusId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new vehicle to the data store.
    /// </summary>
    Task AddAsync(
        Vehicle vehicle,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists pending vehicle changes.
    /// </summary>
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all vehicle categories.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The list of available vehicle categories.
    /// </returns>
    Task<List<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all vehicle statuses.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The list of available vehicle statuses.
    /// </returns>
    Task<List<VehicleStatus>> GetVehicleStatusesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a vehicle status by its name.
    /// </summary>
    /// <param name="name">
    /// The name of the vehicle status to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The matching vehicle status when it exists;
    /// otherwise, null.
    /// </returns>
    Task<VehicleStatus?> GetVehicleStatusByNameAsync(
        string name,
        CancellationToken cancellationToken = default);
}
