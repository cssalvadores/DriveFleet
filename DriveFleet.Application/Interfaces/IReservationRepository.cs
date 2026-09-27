using DriveFleet.Domain.Entities;
using DriveFleet.Application.Filters;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines the persistence operations required for reservation management.
/// </summary>
public interface IReservationRepository
{
    /// <summary>
    /// Gets a reservation by its unique identifier, including its related details.
    /// </summary>
    /// <param name="reservationId">The reservation identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The reservation when found; otherwise, null.</returns>
    /// <summary>
    /// Gets a reservation by its unique identifier,
    /// including its related details.
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
    Task<Reservation?> GetByIdAsync(
        int reservationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets reservations belonging to a specific user
    /// that match the supplied filter.
    /// </summary>
    /// <param name="userId">
    /// The user identifier.
    /// </param>
    /// <param name="filter">
    /// The reservation filtering criteria.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection of matching reservations.
    /// </returns>
    Task<IReadOnlyList<Reservation>> GetByUserIdAsync(
        int userId,
        ReservationFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all reservations that match
    /// the supplied filter.
    /// </summary>
    /// <param name="filter">
    /// The reservation filtering criteria.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection containing
    /// the matching reservations.
    /// </returns>
    Task<IReadOnlyList<Reservation>> GetAllAsync(
        ReservationFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a vehicle has a non-cancelled
    /// reservation that overlaps the specified date range.
    /// </summary>
    /// <param name="vehicleId">
    /// The vehicle identifier.
    /// </param>
    /// <param name="startDate">
    /// The requested reservation start date.
    /// </param>
    /// <param name="endDate">
    /// The requested reservation end date.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// True when an overlapping reservation exists;
    /// otherwise, false.
    /// </returns>
    Task<bool> HasVehicleOverlapAsync(
        int vehicleId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new reservation to the current
    /// persistence context.
    /// </summary>
    /// <param name="reservation">
    /// The reservation to add.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    Task AddAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
