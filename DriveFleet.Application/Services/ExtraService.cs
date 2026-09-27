using DriveFleet.Application.DTOs.Extras;
using DriveFleet.Application.Interfaces;
using DriveFleet.Domain.Entities;

namespace DriveFleet.Application.Services;

/// <summary>
/// Provides application operations for reservation extras.
/// </summary>
public class ExtraService : IExtraService
{
    private readonly IExtraRepository _extraRepository;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ExtraService"/> class.
    /// </summary>
    /// <param name="extraRepository">
    /// Repository used to access extra data.
    /// </param>
    public ExtraService(
        IExtraRepository extraRepository)
    {
        _extraRepository = extraRepository;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ExtraResponse>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var extras =
            await _extraRepository.GetActiveAsync(
                cancellationToken);

        return extras
            .Select(MapToResponse)
            .ToList();
    }

    /// <summary>
    /// Maps an extra entity to the response returned to clients.
    /// </summary>
    /// <param name="extra">The extra entity.</param>
    /// <returns>The mapped extra response.</returns>
    private static ExtraResponse MapToResponse(
        Extra extra)
    {
        return new ExtraResponse
        {
            ExtraId = extra.ExtraId,
            Name = extra.Name,
            Description = extra.Description,
            Price = extra.Price,
            Photo = extra.Photo
        };
    }
}
