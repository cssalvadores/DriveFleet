using DriveFleet.Application.DTOs.Vehicles;
using DriveFleet.Application.Exceptions;
using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Entities;

namespace DriveFleet.Application.Services;

/// <summary>
/// Provides application operations for vehicle management.
/// </summary>
public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;

    /// <summary>
    /// Storage service used to persist vehicle catalog photos.
    /// </summary>
    private readonly IVehiclePhotoStorage _vehiclePhotoStorage;

    /// <summary>
    /// Represents the vehicle status used when a vehicle
    /// is removed from active availability.
    /// </summary>
    private const string UnavailableStatusName = "Unavailable";

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VehicleService"/> class.
    /// </summary>
    /// <param name="vehicleRepository">
    /// Repository used to access and persist vehicle data.
    /// </param>
    /// <param name="vehiclePhotoStorage">
    /// Storage service used to save and delete vehicle catalog photos.
    /// </param>
    public VehicleService(
    IVehicleRepository vehicleRepository,
    IVehiclePhotoStorage vehiclePhotoStorage)
    {
        _vehicleRepository = vehicleRepository;
        _vehiclePhotoStorage = vehiclePhotoStorage;
    }

    /// <summary>
    /// Returns all vehicles.
    /// </summary>
    public async Task<List<VehicleResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var vehicles =
            await _vehicleRepository.GetAllAsync(
                cancellationToken);

        return vehicles
            .Select(MapToResponse)
            .ToList();
    }

    /// <summary>
    /// Returns a vehicle by its identifier.
    /// </summary>
    public async Task<VehicleResponse?> GetByIdAsync(
        int vehicleId,
        CancellationToken cancellationToken = default)
    {
        var vehicle =
            await _vehicleRepository.GetByIdAsync(
                vehicleId,
                cancellationToken);

        return vehicle is null
            ? null
            : MapToResponse(vehicle);
    }

    /// <summary>
    /// Retrieves the available vehicle categories.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The list of vehicle category options.
    /// </returns>
    public async Task<List<VehicleOptionResponse>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        var categories =
            await _vehicleRepository.GetCategoriesAsync(
                cancellationToken);

        return categories
            .Select(category => new VehicleOptionResponse
            {
                Id = category.CategoryId,
                Name = category.Name
            })
            .ToList();
    }

    /// <summary>
    /// Retrieves the available vehicle statuses.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The list of vehicle status options.
    /// </returns>
    public async Task<List<VehicleOptionResponse>> GetVehicleStatusesAsync(
        CancellationToken cancellationToken = default)
    {
        var statuses =
            await _vehicleRepository.GetVehicleStatusesAsync(
                cancellationToken);

        return statuses
            .Select(status => new VehicleOptionResponse
            {
                Id = status.VehicleStatusId,
                Name = status.Name
            })
            .ToList();
    }

    /// <summary>
    /// Creates a new vehicle.
    /// </summary>
    public async Task<VehicleResponse> CreateAsync(
        CreateVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var licensePlate =
            request.LicensePlate.Trim();

        var licensePlateExists =
            await _vehicleRepository.LicensePlateExistsAsync(
                licensePlate,
                cancellationToken: cancellationToken);

        if (licensePlateExists)
        {
            throw new ConflictException(
                "A vehicle with this license plate already exists.");
        }

        var categoryExists =
        await _vehicleRepository.CategoryExistsAsync(
        request.CategoryId,
        cancellationToken);

        if (!categoryExists)
        {
            throw new InvalidVehicleReferenceException(
                "The specified vehicle category does not exist.");
        }

        var vehicleStatusExists =
            await _vehicleRepository.VehicleStatusExistsAsync(
                request.VehicleStatusId,
                cancellationToken);

        if (!vehicleStatusExists)
        {
            throw new InvalidVehicleReferenceException(
                "The specified vehicle status does not exist.");
        }

        var vehicle = new Vehicle
        {
            Brand = request.Brand.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            LicensePlate = licensePlate,
            Seats = request.Seats,
            DailyPrice = request.DailyPrice,
            Description = string.IsNullOrWhiteSpace(
                request.Description)
                ? null
                : request.Description.Trim(),
            CategoryId = request.CategoryId,
            VehicleStatusId = request.VehicleStatusId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        await _vehicleRepository.AddAsync(
            vehicle,
            cancellationToken);

        await _vehicleRepository.SaveChangesAsync(
            cancellationToken);

        var createdVehicle =
            await _vehicleRepository.GetByIdAsync(
                vehicle.VehicleId,
                cancellationToken);

        if (createdVehicle is null)
        {
            throw new InvalidOperationException(
                "The created vehicle could not be retrieved.");
        }

        return MapToResponse(createdVehicle);
    }

    /// <summary>
    /// Updates an existing vehicle.
    /// </summary>
    public async Task<bool> UpdateAsync(
        int vehicleId,
        UpdateVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var vehicle =
            await _vehicleRepository.GetByIdAsync(
                vehicleId,
                cancellationToken);

        if (vehicle is null)
        {
            return false;
        }

        var licensePlate =
            request.LicensePlate.Trim();

        var licensePlateExists =
            await _vehicleRepository.LicensePlateExistsAsync(
                licensePlate,
                vehicleId,
                cancellationToken);

        if (licensePlateExists)
        {
            throw new ConflictException(
                "A vehicle with this license plate already exists.");
        }

        var categoryExists =
        await _vehicleRepository.CategoryExistsAsync(
        request.CategoryId,
        cancellationToken);

        if (!categoryExists)
        {
            throw new InvalidVehicleReferenceException(
                "The specified vehicle category does not exist.");
        }

        var vehicleStatusExists =
            await _vehicleRepository.VehicleStatusExistsAsync(
                request.VehicleStatusId,
                cancellationToken);

        if (!vehicleStatusExists)
        {
            throw new InvalidVehicleReferenceException(
                "The specified vehicle status does not exist.");
        }

        vehicle.Brand = request.Brand.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.Year = request.Year;
        vehicle.LicensePlate = licensePlate;
        vehicle.Seats = request.Seats;
        vehicle.DailyPrice = request.DailyPrice;
        vehicle.Description =
            string.IsNullOrWhiteSpace(
                request.Description)
                ? null
                : request.Description.Trim();
        vehicle.CategoryId = request.CategoryId;
        vehicle.VehicleStatusId =
            request.VehicleStatusId;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _vehicleRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

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
    /// <exception cref="InvalidVehicleReferenceException">
    /// Thrown when the unavailable vehicle status is not configured.
    /// </exception>
    public async Task<bool> MarkUnavailableAsync(
        int vehicleId,
        CancellationToken cancellationToken = default)
    {
        var vehicle =
            await _vehicleRepository.GetByIdAsync(
                vehicleId,
                cancellationToken);

        if (vehicle is null)
        {
            return false;
        }

        var unavailableStatus =
            await _vehicleRepository.GetVehicleStatusByNameAsync(
                UnavailableStatusName,
                cancellationToken);

        if (unavailableStatus is null)
        {
            throw new InvalidVehicleReferenceException(
                "The unavailable vehicle status is not configured.");
        }

        vehicle.VehicleStatusId =
            unavailableStatus.VehicleStatusId;

        vehicle.UpdatedAt =
            DateTime.UtcNow;

        await _vehicleRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

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
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the display position is outside the supported range.
    /// </exception>
    public async Task<VehiclePhotoResponse?> SetPhotoAsync(
        int vehicleId,
        int displayOrder,
        Stream photoStream,
        string fileExtension,
        CancellationToken cancellationToken = default)
    {
        if (displayOrder is < 1 or > 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder),
                "The display order must be between 1 and 3.");
        }

        var vehicle =
            await _vehicleRepository.GetByIdAsync(
                vehicleId,
                cancellationToken);

        if (vehicle is null)
        {
            return null;
        }

        var existingPhoto =
            vehicle.VehiclePhotos.FirstOrDefault(
                photo => photo.DisplayOrder == displayOrder);

        var previousPhotoPath =
            existingPhoto?.FilePath;

        var newPhotoPath =
            await _vehiclePhotoStorage.SaveAsync(
                vehicleId,
                photoStream,
                fileExtension,
                cancellationToken);

        if (existingPhoto is null)
        {
            existingPhoto = new VehiclePhoto
            {
                VehicleId = vehicleId,
                FilePath = newPhotoPath,
                DisplayOrder = displayOrder,
                CreatedAt = DateTime.UtcNow
            };

            vehicle.VehiclePhotos.Add(
                existingPhoto);
        }
        else
        {
            existingPhoto.FilePath =
                newPhotoPath;
        }

        await _vehicleRepository.SaveChangesAsync(
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousPhotoPath))
        {
            await _vehiclePhotoStorage.DeleteAsync(
                previousPhotoPath,
                cancellationToken);
        }

        return new VehiclePhotoResponse
        {
            VehiclePhotoId =
                existingPhoto.VehiclePhotoId,
            FilePath =
                existingPhoto.FilePath,
            DisplayOrder =
                existingPhoto.DisplayOrder
        };
    }

    /// <summary>
    /// Maps a vehicle entity to the response returned to clients.
    /// </summary>
    private static VehicleResponse MapToResponse(
        Vehicle vehicle)
    {
        return new VehicleResponse
        {
            VehicleId = vehicle.VehicleId,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            LicensePlate = vehicle.LicensePlate,
            Seats = vehicle.Seats,
            DailyPrice = vehicle.DailyPrice,
            Photos = vehicle.VehiclePhotos
                .OrderBy(photo => photo.DisplayOrder)
                .Select(photo => new VehiclePhotoResponse
                {
            VehiclePhotoId = photo.VehiclePhotoId,
            FilePath = photo.FilePath,
            DisplayOrder = photo.DisplayOrder
                })
                .ToList(),
            Description = vehicle.Description,
            CategoryId = vehicle.CategoryId,
            CategoryName = vehicle.Category.Name,
            VehicleStatusId = vehicle.VehicleStatusId,
            VehicleStatusName =
                vehicle.VehicleStatus.Name,
            CreatedAt = vehicle.CreatedAt,
            UpdatedAt = vehicle.UpdatedAt
        };
    }
}
