using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Entities;
using DriveFleet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriveFleet.Infrastructure.Repositories;

/// <summary>
/// Provides database operations for vehicle reviews.
/// </summary>
public class ReviewRepository : IReviewRepository
{
    private readonly DriveFleetDbContext _context;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReviewRepository"/> class.
    /// </summary>
    public ReviewRepository(
        DriveFleetDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<Review?> GetByReservationVehicleIdAsync(
        int reservationVehicleId,
        CancellationToken cancellationToken = default)
    {
        return _context.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(
                review =>
                    review.ReservationVehicleId ==
                    reservationVehicleId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(
        Review review,
        CancellationToken cancellationToken = default)
    {
        await _context.Reviews.AddAsync(
            review,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Review>>
        GetVisibleByVehicleIdAsync(
            int vehicleId,
            CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Where(review =>
                review.IsVisible &&
                review.ReservationVehicle.VehicleId ==
                vehicleId)
            .OrderByDescending(review =>
                review.CreatedAt)
            .ToListAsync(
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Review>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Include(review =>
                review.ReservationVehicle)
            .OrderByDescending(review =>
                review.CreatedAt)
            .ToListAsync(
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<Review?> GetByIdAsync(
        int reviewId,
        CancellationToken cancellationToken = default)
    {
        return _context.Reviews
            .Include(review =>
                review.ReservationVehicle)
            .FirstOrDefaultAsync(
                review =>
                    review.ReviewId == reviewId,
                cancellationToken);
    }

    /// <inheritdoc />
    public void Remove(
        Review review)
    {
        _context.Reviews.Remove(
            review);
    }
}
