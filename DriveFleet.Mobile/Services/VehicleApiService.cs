using System.Net.Http.Headers;
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
    public async Task<VehicleListApiResult> GetAllAsync(
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
    /// Retrieves a vehicle by its identifier
    /// from the DriveFleet API.
    /// </summary>
    public async Task<VehicleApiResult>GetByIdAsync(
            int vehicleId,
            CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                $"api/vehicles/{vehicleId}",
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var vehicle =
                await response.Content
                    .ReadFromJsonAsync<VehicleModel>(
                        cancellationToken:
                            cancellationToken);

            return new VehicleApiResult
            {
                StatusCode =
                    response.StatusCode,

                Vehicle =
                    vehicle
            };
        }

        return new VehicleApiResult
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
    /// Updates the operational status of an existing
    /// vehicle while preserving its remaining data.
    /// </summary>
    public async Task<UpdateVehicleApiResult>UpdateStatusAsync(
            string accessToken,
            VehicleModel vehicle,
            int vehicleStatusId,
            CancellationToken cancellationToken = default)
    {
        var updateRequest =
            new UpdateVehicleRequest
            {
                Brand =
                    vehicle.Brand,

                Model =
                    vehicle.Model,

                Year =
                    vehicle.Year,

                LicensePlate =
                    vehicle.LicensePlate,

                Seats =
                    vehicle.Seats,

                DailyPrice =
                    vehicle.DailyPrice,

                Description =
                    vehicle.Description,

                CategoryId =
                    vehicle.CategoryId,

                VehicleStatusId =
                    vehicleStatusId
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                $"api/vehicles/{vehicle.VehicleId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        request.Content =
            JsonContent.Create(
                updateRequest);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        return new UpdateVehicleApiResult
        {
            StatusCode =
                response.StatusCode,

            Detail =
                response.IsSuccessStatusCode
                    ? null
                    : await ReadProblemDetailAsync(
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
    private static async Task<string?> ReadProblemDetailAsync(
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
    /// Represents the vehicle information accepted
    /// by the DriveFleet vehicle update endpoint.
    /// </summary>
    private sealed class UpdateVehicleRequest
    {
        public string Brand { get; set; } =
            string.Empty;

        public string Model { get; set; } =
            string.Empty;

        public int Year { get; set; }

        public string LicensePlate { get; set; } =
            string.Empty;

        public int Seats { get; set; }

        public decimal DailyPrice { get; set; }

        public string? Description { get; set; }

        public int CategoryId { get; set; }

        public int VehicleStatusId { get; set; }
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
