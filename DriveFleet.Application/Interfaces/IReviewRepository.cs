using DriveFleet.Domain.Entities;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines persistence operations for vehicle reviews.
/// </summary>
public interface IReviewRepository
{
    /// <summary>
    /// Gets the review associated with a reserved vehicle.
    /// </summary>
    Task<Review?> GetByReservationVehicleIdAsync(
        int reservationVehicleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new review.
    /// </summary>
    Task AddAsync(
        Review review,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists pending review changes.
    /// </summary>
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
