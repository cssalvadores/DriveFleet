using DriveFleet.Application.DTOs.Extras;

namespace DriveFleet.Application.Interfaces;

/// <summary>
/// Defines the application operations available for reservation extras.
/// </summary>
public interface IExtraService
{
    /// <summary>
    /// Gets all active extras available for selection.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>A read-only collection of active extras.</returns>
    Task<IReadOnlyList<ExtraResponse>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}
