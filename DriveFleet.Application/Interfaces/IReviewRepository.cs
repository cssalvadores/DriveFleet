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

    Task<IReadOnlyList<Review>> GetVisibleByVehicleIdAsync(
    int vehicleId,
    CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all reviews for administrative moderation.
    /// </summary>
    Task<IReadOnlyList<Review>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a review by its identifier for modification.
    /// </summary>
    Task<Review?> GetByIdAsync(
        int reviewId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new review.
    /// </summary>
    Task AddAsync(
        Review review,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an existing review.
    /// </summary>
    void Remove(
    Review review);

    /// <summary>
    /// Persists pending review changes.
    /// </summary>
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
        
}
