using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Entities;
using DriveFleet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriveFleet.Infrastructure.Repositories;

public sealed class PasswordResetTokenRepository
    : IPasswordResetTokenRepository
{
    private readonly DriveFleetDbContext _dbContext;

    public PasswordResetTokenRepository(
        DriveFleetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        PasswordResetToken token,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.PasswordResetTokens.AddAsync(
            token,
            cancellationToken);
    }

    public async Task<PasswordResetToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task<IReadOnlyList<PasswordResetToken>>
        GetUnusedByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.PasswordResetTokens
            .Where(
                token =>
                    token.UserId == userId &&
                    token.UsedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}


