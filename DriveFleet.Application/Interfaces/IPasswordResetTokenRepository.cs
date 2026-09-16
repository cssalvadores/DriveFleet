using DriveFleet.Domain.Entities;

namespace DriveFleet.Application.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(
        PasswordResetToken token,
        CancellationToken cancellationToken = default);

    Task<PasswordResetToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PasswordResetToken>> GetUnusedByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
