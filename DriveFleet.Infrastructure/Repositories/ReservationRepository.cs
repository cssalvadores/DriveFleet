using DriveFleet.Application.Filters;
using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Constants;
using DriveFleet.Domain.Entities;
using DriveFleet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriveFleet.Infrastructure.Repositories;

/// <summary>
/// Provides Entity Framework Core persistence
/// operations for reservations.
/// </summary>
public class ReservationRepository : IReservationRepository
{
    private readonly DriveFleetDbContext _context;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReservationRepository"/> class.
    /// </summary>
    /// <param name="context">
    /// The DriveFleet database context.
    /// </param>
    public ReservationRepository(
        DriveFleetDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Reservation?> GetByIdAsync(
        int reservationId,
        CancellationToken cancellationToken = default)
    {
        return await CreateDetailsQuery()
            .FirstOrDefaultAsync(
                reservation =>
                    reservation.ReservationId == reservationId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Reservation>> GetByUserIdAsync(
        int userId,
        ReservationFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query =
            CreateDetailsQuery()
                .AsNoTracking()
                .Where(reservation =>
                    reservation.UserId == userId);

        query =
            ApplyFilter(
                query,
                filter);

        return await query
            .OrderByDescending(reservation =>
                reservation.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Reservation>> GetAllAsync(
        ReservationFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query =
            CreateDetailsQuery()
                .AsNoTracking();

        query =
            ApplyFilter(
                query,
                filter);

        return await query
            .OrderByDescending(reservation =>
                reservation.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasVehicleOverlapAsync(
        int vehicleId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<ReservationVehicle>()
            .AsNoTracking()
            .AnyAsync(
                reservationVehicle =>
                    reservationVehicle.VehicleId == vehicleId
                    && reservationVehicle.Reservation
                        .ReservationStatusId
                        != ReservationStatusIds.Cancelled
                    && reservationVehicle.StartDate < endDate
                    && reservationVehicle.EndDate > startDate,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<Reservation>()
            .AddAsync(
                reservation,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(
            cancellationToken);
    }

    /// <summary>
    /// Applies the reservation filtering criteria
    /// to the supplied query.
    /// </summary>
    /// <param name="query">
    /// The reservation query to filter.
    /// </param>
    /// <param name="filter">
    /// The validated filtering criteria.
    /// </param>
    /// <returns>
    /// The filtered reservation query.
    /// </returns>
    private static IQueryable<Reservation> ApplyFilter(
        IQueryable<Reservation> query,
        ReservationFilter filter)
    {
        if (filter.StatusId.HasValue)
        {
            var statusId =
                filter.StatusId.Value;

            query =
                query.Where(reservation =>
                    reservation.ReservationStatusId ==
                    statusId);
        }

        if (filter.FromDate.HasValue &&
            filter.ToDate.HasValue)
        {
            var fromDate =
                filter.FromDate.Value;

            var toDateExclusive =
                filter.ToDate.Value.AddDays(1);

            return query.Where(reservation =>
                reservation.ReservationVehicles.Any(vehicle =>
                    vehicle.StartDate < toDateExclusive &&
                    vehicle.EndDate > fromDate));
        }

        if (filter.FromDate.HasValue)
        {
            var fromDate =
                filter.FromDate.Value;

            query =
                query.Where(reservation =>
                    reservation.ReservationVehicles.Any(vehicle =>
                        vehicle.EndDate > fromDate));
        }

        if (filter.ToDate.HasValue)
        {
            var toDateExclusive =
                filter.ToDate.Value.AddDays(1);

            query =
                query.Where(reservation =>
                    reservation.ReservationVehicles.Any(vehicle =>
                        vehicle.StartDate < toDateExclusive));
        }

        return query;
    }

    /// <summary>
    /// Creates the base reservation query with the related data
    /// required for reservation details and list operations.
    /// </summary>
    /// <returns>
    /// A query containing the reservation user, status,
    /// vehicles and extras.
    /// </returns>
    private IQueryable<Reservation> CreateDetailsQuery()
    {
        return _context.Set<Reservation>()
            .Include(reservation =>
                reservation.User)
            .Include(reservation =>
                reservation.ReservationStatus)
            .Include(reservation =>
                reservation.ReservationVehicles)
                .ThenInclude(reservationVehicle =>
                    reservationVehicle.Vehicle)
            .Include(reservation =>
                reservation.ReservationVehicles)
                .ThenInclude(reservationVehicle =>
                    reservationVehicle.ReservationVehicleExtras)
                .ThenInclude(reservationVehicleExtra =>
                    reservationVehicleExtra.Extra);
    }
}
