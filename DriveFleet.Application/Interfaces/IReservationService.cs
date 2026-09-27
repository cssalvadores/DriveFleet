using DriveFleet.Application.DTOs.Reservations;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines the application operations available
/// for reservation management.
/// </summary>
public interface IReservationService
{
    /// <summary>
    /// Creates a reservation for the specified user.
    /// </summary>
    /// <param name="userId">
    /// The authenticated user identifier.
    /// </param>
    /// <param name="request">
    /// The reservation creation data.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The created reservation.
    /// </returns>
    Task<ReservationResponse> CreateAsync(
        int userId,
        CreateReservationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a reservation by its identifier.
    /// </summary>
    /// <param name="reservationId">
    /// The reservation identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The reservation when found; otherwise, null.
    /// </returns>
    Task<ReservationResponse?> GetByIdAsync(
        int reservationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets reservations belonging to a specific user
    /// that match the supplied filter.
    /// </summary>
    /// <param name="userId">
    /// The user identifier.
    /// </param>
    /// <param name="filterRequest">
    /// The optional reservation filtering criteria.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection of matching reservations.
    /// </returns>
    Task<IReadOnlyList<ReservationResponse>> GetByUserIdAsync(
        int userId,
        ReservationFilterRequest filterRequest,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all reservations that match
    /// the supplied filter.
    /// </summary>
    /// <param name="filterRequest">
    /// The optional reservation filtering criteria.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection containing
    /// the matching reservations.
    /// </returns>
    Task<IReadOnlyList<ReservationResponse>> GetAllAsync(
        ReservationFilterRequest filterRequest,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a pending reservation.
    /// </summary>
    /// <param name="reservationId">
    /// The reservation identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// True when the reservation is started;
    /// otherwise, false when it does not exist.
    /// </returns>
    Task<bool> StartAsync(
        int reservationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes an active reservation.
    /// </summary>
    /// <param name="reservationId">
    /// The reservation identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// True when the reservation is completed;
    /// otherwise, false when it does not exist.
    /// </returns>
    Task<bool> CompleteAsync(
        int reservationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an existing reservation.
    /// </summary>
    /// <param name="reservationId">
    /// The reservation identifier.
    /// </param>
    /// <param name="userId">
    /// The authenticated user identifier.
    /// </param>
    /// <param name="canManageAllReservations">
    /// Indicates whether the authenticated user may manage
    /// reservations belonging to other users.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// True when the reservation was cancelled;
    /// otherwise, false.
    /// </returns>
    Task<bool> CancelAsync(
        int reservationId,
        int userId,
        bool canManageAllReservations,
        CancellationToken cancellationToken = default);
}
