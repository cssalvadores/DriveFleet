using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Entities;
using DriveFleet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriveFleet.Infrastructure.Repositories;

/// <summary>
/// Provides persistence operations for vehicles
/// using Entity Framework Core.
/// </summary>
public class VehicleRepository : IVehicleRepository
{
    private readonly DriveFleetDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VehicleRepository"/> class.
    /// </summary>
    /// <param name="dbContext">
    /// The database context used to access vehicle data.
    /// </param>
    public VehicleRepository(
        DriveFleetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns all vehicles with their category and current status.
    /// </summary>
    public async Task<List<Vehicle>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.Category)
            .Include(vehicle => vehicle.VehicleStatus)
            .Include(
                vehicle => vehicle.VehiclePhotos
                    .OrderBy(photo => photo.DisplayOrder))
            .OrderBy(vehicle => vehicle.Brand)
            .ThenBy(vehicle => vehicle.Model)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Returns a vehicle by its identifier.
    /// </summary>
    public async Task<Vehicle?> GetByIdAsync(
        int vehicleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Vehicles
            .Include(vehicle => vehicle.Category)
            .Include(vehicle => vehicle.VehicleStatus)
            .Include(
                vehicle => vehicle.VehiclePhotos
                    .OrderBy(photo => photo.DisplayOrder))
            .FirstOrDefaultAsync(
                vehicle => vehicle.VehicleId == vehicleId,
                cancellationToken);
    }

    /// <summary>
    /// Retrieves all vehicle categories ordered by name.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    /// <returns>
    /// The list of vehicle categories.
    /// </returns>
    public async Task<List<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all vehicle statuses ordered by name.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    /// <returns>
    /// The list of vehicle statuses.
    /// </returns>
    public async Task<List<VehicleStatus>> GetVehicleStatusesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.VehicleStatuses
            .AsNoTracking()
            .OrderBy(status => status.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a vehicle status by its name.
    /// </summary>
    /// <param name="name">
    /// The name of the vehicle status to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    /// <returns>
    /// The matching vehicle status when it exists;
    /// otherwise, null.
    /// </returns>
    public async Task<VehicleStatus?> GetVehicleStatusByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.VehicleStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                status => status.Name == name,
                cancellationToken);
    }

    /// <summary>
    /// Determines whether a license plate is already registered.
    /// </summary>
    public async Task<bool> LicensePlateExistsAsync(
        string licensePlate,
        int? excludeVehicleId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle =>
                vehicle.LicensePlate == licensePlate);

        if (excludeVehicleId.HasValue)
        {
            query = query.Where(
                vehicle =>
                    vehicle.VehicleId != excludeVehicleId.Value);
        }

        return await query.AnyAsync(
            cancellationToken);
    }

    /// <summary>
    /// Determines whether a vehicle category exists.
    /// </summary>
    public async Task<bool> CategoryExistsAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .AnyAsync(
                category => category.CategoryId == categoryId,
                cancellationToken);
    }

    /// <summary>
    /// Determines whether a vehicle status exists.
    /// </summary>
    public async Task<bool> VehicleStatusExistsAsync(
        int vehicleStatusId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.VehicleStatuses
            .AsNoTracking()
            .AnyAsync(
                status => status.VehicleStatusId == vehicleStatusId,
                cancellationToken);
    }

    /// <summary>
    /// Adds a new vehicle to the data store.
    /// </summary>
    public async Task AddAsync(
        Vehicle vehicle,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Vehicles.AddAsync(
            vehicle,
            cancellationToken);
    }

    /// <summary>
    /// Persists pending vehicle changes.
    /// </summary>
    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
