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
}