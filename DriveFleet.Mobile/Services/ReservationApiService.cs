using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DriveFleet.Mobile.Models.Reservations;

namespace DriveFleet.Mobile.Services;

/// <summary>
/// Provides reservation operations against
/// the DriveFleet REST API.
/// </summary>
public class ReservationApiService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReservationApiService"/> class.
    /// </summary>
    public ReservationApiService(
        HttpClient httpClient)
    {
        _httpClient =
            httpClient;
    }

    /// <summary>
    /// Retrieves reservations associated
    /// with the supplied calendar date.
    /// </summary>
    public async Task<ReservationListApiResult>
        GetByDateAsync(
            string accessToken,
            DateTime date,
            CancellationToken cancellationToken = default)
    {
        var formattedDate =
            date.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);

        var requestUri =
            "api/reservations" +
            $"?fromDate={formattedDate}" +
            $"&toDate={formattedDate}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                requestUri);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var reservations =
                await response.Content
                    .ReadFromJsonAsync<List<ReservationModel>>(
                        cancellationToken:
                            cancellationToken);

            return new ReservationListApiResult
            {
                StatusCode =
                    response.StatusCode,

                Reservations =
                    reservations ??
                    new List<ReservationModel>()
            };
        }

        return new ReservationListApiResult
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
    /// Reads ProblemDetails information returned
    /// by the DriveFleet API.
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
