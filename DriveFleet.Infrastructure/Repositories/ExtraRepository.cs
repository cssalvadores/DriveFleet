using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Entities;
using DriveFleet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriveFleet.Infrastructure.Repositories;

/// <summary>
/// Provides Entity Framework Core persistence operations for extras.
/// </summary>
public class ExtraRepository : IExtraRepository
{
    private readonly DriveFleetDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraRepository"/> class.
    /// </summary>
    /// <param name="context">The DriveFleet database context.</param>
    public ExtraRepository(DriveFleetDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Extra>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Extra>()
            .AsNoTracking()
            .Where(extra => extra.Active)
            .OrderBy(extra => extra.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Extra>> GetActiveByIdsAsync(
        IReadOnlyCollection<int> extraIds,
        CancellationToken cancellationToken = default)
    {
        if (extraIds.Count == 0)
        {
            return Array.Empty<Extra>();
        }

        return await _context.Set<Extra>()
            .AsNoTracking()
            .Where(extra =>
                extra.Active &&
                extraIds.Contains(extra.ExtraId))
            .ToListAsync(cancellationToken);
    }
}
