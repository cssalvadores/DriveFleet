using DriveFleet.Application.DTOs.Reviews;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines application operations for vehicle reviews.
/// </summary>
public interface IReviewService
{
    /// <summary>
    /// Creates a review for a vehicle from a completed reservation.
    /// </summary>
    Task<ReviewResponse?> CreateAsync(
        int userId,
        int reservationId,
        int reservationVehicleId,
        CreateReviewRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the review created for a vehicle
    /// in one of the client's reservations.
    /// </summary>
    Task<ReviewResponse?> GetByReservationVehicleAsync(
        int userId,
        int reservationId,
        int reservationVehicleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the reviews that are publicly visible
    /// for the specified vehicle.
    /// </summary>
    Task<IReadOnlyList<ReviewResponse>>
        GetVisibleByVehicleIdAsync(
            int vehicleId,
            CancellationToken cancellationToken = default);
}