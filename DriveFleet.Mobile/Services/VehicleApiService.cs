using System.Net.Http.Json;
using System.Text.Json;
using DriveFleet.Mobile.Models.Vehicles;

namespace DriveFleet.Mobile.Services;

/// <summary>
/// Provides vehicle operations against
/// the DriveFleet REST API.
/// </summary>
public class VehicleApiService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VehicleApiService"/> class.
    /// </summary>
    public VehicleApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Retrieves all vehicles from
    /// the DriveFleet API.
    /// </summary>
    public async Task<VehicleListApiResult>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                "api/vehicles",
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var vehicles =
                await response.Content
                    .ReadFromJsonAsync<List<VehicleModel>>(
                        cancellationToken:
                            cancellationToken);

            return new VehicleListApiResult
            {
                StatusCode =
                    response.StatusCode,

                Vehicles =
                    vehicles ??
                    new List<VehicleModel>()
            };
        }

        return new VehicleListApiResult
        {
            StatusCode =
                response.StatusCode,

            Detail =
                await ReadProblemDetailAsync(
                    response,
                    cancellationToken)
        };
    }

    /// <summary>
    /// Builds an absolute URL for a public
    /// resource exposed by the API.
    /// </summary>
    public string? GetPublicResourceUrl(
        string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(
                relativePath) ||
            _httpClient.BaseAddress is null)
        {
            return null;
        }

        return new Uri(
            _httpClient.BaseAddress,
            relativePath.TrimStart('/'))
            .ToString();
    }

    /// <summary>
    /// Reads ProblemDetails information
    /// returned by the API.
    /// </summary>
    private static async Task<string?>
        ReadProblemDetailAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
    {
        try
        {
            var problem =
                await response.Content
                    .ReadFromJsonAsync<ApiProblemDetails>(
                        cancellationToken:
                            cancellationToken);

            return problem?.Detail;
        }
        catch (JsonException)
        {
            // Keeps the detail empty when the API response
            // does not contain valid JSON.
            return null;
        }
    }

    /// <summary>
    /// Represents relevant ProblemDetails
    /// information returned by the API.
    /// </summary>
    private sealed class ApiProblemDetails
    {
        public string? Detail { get; set; }
    }
}
