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
            CreateAuthenticatedRequest(
                HttpMethod.Get,
                requestUri,
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
    /// Retrieves all reservations available
    /// to the authenticated employee.
    /// </summary>
    public async Task<ReservationListApiResult>
        GetAllAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
    {
        using var request =
            CreateAuthenticatedRequest(
                HttpMethod.Get,
                "api/reservations",
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
    /// Retrieves a reservation by its identifier.
    /// </summary>
    public async Task<ReservationDetailsApiResult>
        GetByIdAsync(
            string accessToken,
            int reservationId,
            CancellationToken cancellationToken = default)
    {
        using var request =
            CreateAuthenticatedRequest(
                HttpMethod.Get,
                $"api/reservations/{reservationId}",
                accessToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var reservation =
                await response.Content
                    .ReadFromJsonAsync<ReservationModel>(
                        cancellationToken:
                            cancellationToken);

            return new ReservationDetailsApiResult
            {
                StatusCode =
                    response.StatusCode,

                Reservation =
                    reservation
            };
        }

        return new ReservationDetailsApiResult
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
    /// Starts a pending reservation.
    /// </summary>
    public Task<ReservationActionApiResult>
        StartAsync(
            string accessToken,
            int reservationId,
            CancellationToken cancellationToken = default)
    {
        return SendReservationActionAsync(
            accessToken,
            reservationId,
            "start",
            cancellationToken);
    }

    /// <summary>
    /// Completes an active reservation.
    /// </summary>
    public Task<ReservationActionApiResult>
        CompleteAsync(
            string accessToken,
            int reservationId,
            CancellationToken cancellationToken = default)
    {
        return SendReservationActionAsync(
            accessToken,
            reservationId,
            "complete",
            cancellationToken);
    }

    /// <summary>
    /// Sends an authenticated reservation
    /// workflow action to the DriveFleet API.
    /// </summary>
    private async Task<ReservationActionApiResult>
        SendReservationActionAsync(
            string accessToken,
            int reservationId,
            string actionName,
            CancellationToken cancellationToken)
    {
        using var request =
            CreateAuthenticatedRequest(
                HttpMethod.Patch,
                $"api/reservations/{reservationId}/{actionName}",
                accessToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        return new ReservationActionApiResult
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
    /// Creates an authenticated HTTP request
    /// for the DriveFleet API.
    /// </summary>
    private static HttpRequestMessage
        CreateAuthenticatedRequest(
            HttpMethod method,
            string requestUri,
            string accessToken)
    {
        var request =
            new HttpRequestMessage(
                method,
                requestUri);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        return request;
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
    /// Represents relevant ProblemDetails information
    /// returned by the DriveFleet API.
    /// </summary>
    private sealed class ApiProblemDetails
    {
        public string? Detail { get; set; }
    }
}