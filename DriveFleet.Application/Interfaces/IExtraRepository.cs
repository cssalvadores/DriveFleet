using DriveFleet.Domain.Entities;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines the persistence operations required to access reservation extras.
/// </summary>
public interface IExtraRepository
{
    /// <summary>
    /// Gets all active extras available for new reservations.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection containing all active extras.
    /// </returns>
    Task<IReadOnlyList<Extra>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active extras that match the specified identifiers.
    /// </summary>
    /// <param name="extraIds">The identifiers of the requested extras.</param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection containing the matching active extras.
    /// </returns>
    Task<IReadOnlyList<Extra>> GetActiveByIdsAsync(
        IReadOnlyCollection<int> extraIds,
        CancellationToken cancellationToken = default);
}
