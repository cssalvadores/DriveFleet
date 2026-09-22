using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Entities;
using DriveFleet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriveFleet.Infrastructure.Repositories;

/// <summary>
/// Provides persistence operations for revoked JWT access tokens
/// using Entity Framework Core.
/// </summary>
public class RevokedJwtTokenRepository
    : IRevokedJwtTokenRepository
{
    private readonly DriveFleetDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="RevokedJwtTokenRepository"/> class.
    /// </summary>
    /// <param name="dbContext">
    /// The database context used to access revoked JWT data.
    /// </param>
    public RevokedJwtTokenRepository(
        DriveFleetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Determines whether a JWT identifier has been revoked.
    /// </summary>
    /// <param name="jti">
    /// The unique identifier stored in the JWT jti claim.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    /// <returns>
    /// True if the JWT has been revoked; otherwise, false.
    /// </returns>
    public async Task<bool> IsRevokedAsync(
        string jti,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RevokedJwtTokens
            .AsNoTracking()
            .AnyAsync(
                token => token.Jti == jti,
                cancellationToken);
    }

    /// <summary>
    /// Adds a revoked JWT token and persists it to the database.
    /// </summary>
    /// <param name="token">
    /// The revoked JWT token information to persist.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous database operation if needed.
    /// </param>
    public async Task AddAsync(
        RevokedJwtToken token,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RevokedJwtTokens.AddAsync(
            token,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
