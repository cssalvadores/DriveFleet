using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DriveFleet.Web.Services;

/// <summary>
/// Provides HTTP operations for extra catalog endpoints
/// exposed by the DriveFleet API.
/// </summary>
public class ExtraApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ExtraApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">
    /// The HTTP client configured to communicate with the DriveFleet API.
    /// </param>
    public ExtraApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets all active extras available for new reservations.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request.
    /// </param>
    /// <returns>The active extras result returned by the API.</returns>
    public async Task<ExtraListApiResult> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                "api/extras",
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var extras =
                await response.Content
                    .ReadFromJsonAsync<List<ExtraResponse>>(
                        cancellationToken: cancellationToken);

            return new ExtraListApiResult
            {
                StatusCode = response.StatusCode,
                Extras = extras?
                    .Select(MapExtra)
                    .ToList()
                    ?? new List<ExtraApiModel>()
            };
        }

        return new ExtraListApiResult
        {
            StatusCode = response.StatusCode,
            Detail = await ReadProblemDetailAsync(
                response,
                cancellationToken)
        };
    }

    /// <summary>
    /// Maps an extra returned by the API to the model consumed by the Web project.
    /// </summary>
    /// <param name="extra">The API extra response.</param>
    /// <returns>The mapped extra model.</returns>
    private static ExtraApiModel MapExtra(
        ExtraResponse extra)
    {
        return new ExtraApiModel
        {
            ExtraId = extra.ExtraId,
            Name = extra.Name,
            Description = extra.Description,
            Price = extra.Price,
            Photo = extra.Photo
        };
    }

    /// <summary>
    /// Reads ProblemDetails information returned by the API.
    /// </summary>
    private static async Task<string?> ReadProblemDetailAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var problemDetails =
                await response.Content
                    .ReadFromJsonAsync<ApiProblemDetails>(
                        cancellationToken: cancellationToken);

            return problemDetails?.Detail;
        }
        catch (JsonException)
        {
            // Keeps the detail empty when the API response is not valid JSON.
            return null;
        }
    }

    private sealed class ExtraResponse
    {
        public int ExtraId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? Photo { get; set; }
    }

    private sealed class ApiProblemDetails
    {
        public string? Detail { get; set; }
    }
}

/// <summary>
/// Represents an extra consumed by the Web project.
/// </summary>
public class ExtraApiModel
{
    public int ExtraId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? Photo { get; set; }
}

/// <summary>
/// Represents the result of an extras catalog API request.
/// </summary>
public class ExtraListApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public IReadOnlyList<ExtraApiModel> Extras { get; set; }
        = Array.Empty<ExtraApiModel>();

    public string? Detail { get; set; }
}
