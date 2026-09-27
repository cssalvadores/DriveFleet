using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace DriveFleet.Web.Services;

/// <summary>
/// Provides HTTP operations for reservation endpoints
/// exposed by the DriveFleet API.
/// </summary>
public class ReservationApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReservationApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">
    /// The HTTP client configured to communicate with the DriveFleet API.
    /// </param>
    public ReservationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets the reservations that belong to the authenticated user
    /// and match the supplied filtering criteria.
    /// </summary>
    /// <param name="accessToken">
    /// The authenticated user's JWT access token.
    /// </param>
    /// <param name="statusId">
    /// The optional reservation status identifier.
    /// </param>
    /// <param name="fromDate">
    /// The optional first rental date.
    /// </param>
    /// <param name="toDate">
    /// The optional last rental date.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request.
    /// </param>
    /// <returns>
    /// The reservation list result returned by the API.
    /// </returns>
    public Task<ReservationListApiResult> GetMineAsync(
        string accessToken,
        int? statusId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        return GetListAsync(
            "api/reservations/mine",
            accessToken,
            statusId,
            fromDate,
            toDate,
            cancellationToken);
    }

    /// <summary>
    /// Gets all reservations available to an authenticated
    /// administrator or employee that match the supplied filters.
    /// </summary>
    /// <param name="accessToken">
    /// The authenticated user's JWT access token.
    /// </param>
    /// <param name="statusId">
    /// The optional reservation status identifier.
    /// </param>
    /// <param name="fromDate">
    /// The optional first rental date.
    /// </param>
    /// <param name="toDate">
    /// The optional last rental date.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request.
    /// </param>
    /// <returns>
    /// The reservation list result returned by the API.
    /// </returns>
    public Task<ReservationListApiResult> GetAllAsync(
        string accessToken,
        int? statusId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        return GetListAsync(
            "api/reservations",
            accessToken,
            statusId,
            fromDate,
            toDate,
            cancellationToken);
    }

    /// <summary>
    /// Gets a reservation by its identifier.
    /// </summary>
    /// <param name="accessToken">The authenticated user's JWT access token.</param>
    /// <param name="reservationId">The reservation identifier.</param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request.
    /// </param>
    /// <returns>The reservation details result returned by the API.</returns>
    public async Task<ReservationDetailsApiResult> GetByIdAsync(
        string accessToken,
        int reservationId,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthenticatedRequest(
            HttpMethod.Get,
            $"api/reservations/{reservationId}",
            accessToken);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var reservation =
                await response.Content
                    .ReadFromJsonAsync<ReservationResponse>(
                        cancellationToken: cancellationToken);

            return new ReservationDetailsApiResult
            {
                StatusCode = response.StatusCode,
                Reservation = reservation is null
                    ? null
                    : MapReservation(reservation)
            };
        }

        return new ReservationDetailsApiResult
        {
            StatusCode = response.StatusCode,
            Detail = await ReadProblemDetailAsync(
                response,
                cancellationToken)
        };
    }

    /// <summary>
    /// Creates a new reservation for the authenticated client.
    /// </summary>
    /// <param name="accessToken">The authenticated user's JWT access token.</param>
    /// <param name="vehicles">The vehicles included in the reservation.</param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request.
    /// </param>
    /// <returns>The reservation creation result returned by the API.</returns>
    public async Task<CreateReservationApiResult> CreateAsync(
        string accessToken,
        IReadOnlyCollection<CreateReservationVehicleApiModel> vehicles,
        CancellationToken cancellationToken = default)
    {
        var body = new CreateReservationRequest
        {
            Vehicles = vehicles
                .Select(vehicle => new CreateReservationVehicleRequest
                {
                    VehicleId = vehicle.VehicleId,
                    StartDate = vehicle.StartDate,
                    EndDate = vehicle.EndDate,
                    Extras = vehicle.Extras
                        .Select(extra => new CreateReservationExtraRequest
                        {
                            ExtraId = extra.ExtraId,
                            Quantity = extra.Quantity
                        })
                        .ToList()
                })
                .ToList()
        };

        using var request = CreateAuthenticatedRequest(
            HttpMethod.Post,
            "api/reservations",
            accessToken);

        request.Content = JsonContent.Create(body);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            var reservation =
                await response.Content
                    .ReadFromJsonAsync<ReservationResponse>(
                        cancellationToken: cancellationToken);

            return new CreateReservationApiResult
            {
                StatusCode = response.StatusCode,
                Reservation = reservation is null
                    ? null
                    : MapReservation(reservation)
            };
        }

        return new CreateReservationApiResult
        {
            StatusCode = response.StatusCode,
            Detail = await ReadProblemDetailAsync(
                response,
                cancellationToken)
        };
    }

    /// <summary>
    /// Cancels an existing reservation.
    /// </summary>
    public Task<ReservationActionApiResult> CancelAsync(
        string accessToken,
        int reservationId,
        CancellationToken cancellationToken = default)
    {
        return SendReservationActionAsync(
            accessToken,
            reservationId,
            "cancel",
            cancellationToken);
    }

    /// <summary>
    /// Starts a pending reservation.
    /// </summary>
    public Task<ReservationActionApiResult> StartAsync(
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
    public Task<ReservationActionApiResult> CompleteAsync(
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
    /// Sends a reservation status action
    /// to the DriveFleet API.
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
            StatusCode = response.StatusCode,

            Detail = response.IsSuccessStatusCode
                ? null
                : await ReadProblemDetailAsync(
                    response,
                    cancellationToken)
        };
    }

    /// <summary>
    /// Gets a reservation list from the specified API endpoint
    /// using the supplied filtering criteria.
    /// </summary>
    /// <param name="requestUri">
    /// The base reservation endpoint.
    /// </param>
    /// <param name="accessToken">
    /// The authenticated user's JWT access token.
    /// </param>
    /// <param name="statusId">
    /// The optional reservation status identifier.
    /// </param>
    /// <param name="fromDate">
    /// The optional first rental date.
    /// </param>
    /// <param name="toDate">
    /// The optional last rental date.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request.
    /// </param>
    /// <returns>
    /// The reservation list result returned by the API.
    /// </returns>
    private async Task<ReservationListApiResult> GetListAsync(
        string requestUri,
        string accessToken,
        int? statusId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var filteredRequestUri =
            BuildReservationListRequestUri(
                requestUri,
                statusId,
                fromDate,
                toDate);

        using var request =
            CreateAuthenticatedRequest(
                HttpMethod.Get,
                filteredRequestUri,
                accessToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        return await ReadReservationListResultAsync(
            response,
            cancellationToken);
    }

    /// <summary>
    /// Builds a reservation list URI containing only
    /// the filters supplied by the user.
    /// </summary>
    /// <param name="requestUri">
    /// The base reservation endpoint.
    /// </param>
    /// <param name="statusId">
    /// The optional reservation status identifier.
    /// </param>
    /// <param name="fromDate">
    /// The optional first rental date.
    /// </param>
    /// <param name="toDate">
    /// The optional last rental date.
    /// </param>
    /// <returns>
    /// The reservation endpoint including its query string.
    /// </returns>
    private static string BuildReservationListRequestUri(
        string requestUri,
        int? statusId,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var queryParameters =
            new Dictionary<string, string?>();

        if (statusId.HasValue)
        {
            queryParameters["statusId"] =
                statusId.Value.ToString(
                    CultureInfo.InvariantCulture);
        }

        if (fromDate.HasValue)
        {
            queryParameters["fromDate"] =
                fromDate.Value.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);
        }

        if (toDate.HasValue)
        {
            queryParameters["toDate"] =
                toDate.Value.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);
        }

        return queryParameters.Count == 0
            ? requestUri
            : QueryHelpers.AddQueryString(
                requestUri,
                queryParameters);
    }

    /// <summary>
    /// Creates an HTTP request containing the JWT authorization header.
    /// </summary>
    private static HttpRequestMessage CreateAuthenticatedRequest(
        HttpMethod method,
        string requestUri,
        string accessToken)
    {
        var request = new HttpRequestMessage(
            method,
            requestUri);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        return request;
    }

    /// <summary>
    /// Reads a reservation list response returned by the API.
    /// </summary>
    private static async Task<ReservationListApiResult>
        ReadReservationListResultAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var reservations =
                await response.Content
                    .ReadFromJsonAsync<List<ReservationResponse>>(
                        cancellationToken: cancellationToken);

            return new ReservationListApiResult
            {
                StatusCode = response.StatusCode,
                Reservations = reservations?
                    .Select(MapReservation)
                    .ToList()
                    ?? new List<ReservationApiModel>()
            };
        }

        return new ReservationListApiResult
        {
            StatusCode = response.StatusCode,
            Detail = await ReadProblemDetailAsync(
                response,
                cancellationToken)
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
            var problem =
                await response.Content
                    .ReadFromJsonAsync<ApiProblemDetails>(
                        cancellationToken: cancellationToken);

            return problem?.Detail;
        }
        catch (JsonException)
        {
            // Keeps the detail empty when the API response is not valid JSON.
            return null;
        }
    }

    /// <summary>
    /// Maps an API reservation response to the model consumed by the Web project.
    /// </summary>
    private static ReservationApiModel MapReservation(
        ReservationResponse reservation)
    {
        return new ReservationApiModel
        {
            ReservationId = reservation.ReservationId,
            UserId = reservation.UserId,
            UserFullName = reservation.UserFullName,
            UserEmail = reservation.UserEmail,
            ReservationStatusId = reservation.ReservationStatusId,
            ReservationStatusName = reservation.ReservationStatusName,
            TotalValue = reservation.TotalValue,
            CreatedAt = reservation.CreatedAt,
            UpdatedAt = reservation.UpdatedAt,
            Vehicles = reservation.Vehicles
                .Select(MapReservationVehicle)
                .ToList()
        };
    }

    /// <summary>
    /// Maps a reserved vehicle returned by the API.
    /// </summary>
    private static ReservationVehicleApiModel MapReservationVehicle(
        ReservationVehicleResponse vehicle)
    {
        return new ReservationVehicleApiModel
        {
            ReservationVehicleId = vehicle.ReservationVehicleId,
            VehicleId = vehicle.VehicleId,
            VehicleBrand = vehicle.VehicleBrand,
            VehicleModel = vehicle.VehicleModel,
            LicensePlate = vehicle.LicensePlate,
            StartDate = vehicle.StartDate,
            EndDate = vehicle.EndDate,
            DailyPrice = vehicle.DailyPrice,
            RentalDays = vehicle.RentalDays,
            RentalValue = vehicle.RentalValue,
            ExtrasValue = vehicle.ExtrasValue,
            TotalValue = vehicle.TotalValue,
            Extras = vehicle.Extras
                .Select(extra => new ReservationExtraApiModel
                {
                    ExtraId = extra.ExtraId,
                    Name = extra.Name,
                    Quantity = extra.Quantity,
                    Price = extra.Price,
                    Total = extra.Total
                })
                .ToList()
        };
    }

    private sealed class CreateReservationRequest
    {
        public List<CreateReservationVehicleRequest> Vehicles { get; set; }
            = new();
    }

    private sealed class CreateReservationVehicleRequest
    {
        public int VehicleId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<CreateReservationExtraRequest> Extras { get; set; }
            = new();
    }

    private sealed class CreateReservationExtraRequest
    {
        public int ExtraId { get; set; }

        public int Quantity { get; set; }
    }

    private sealed class ReservationResponse
    {
        public int ReservationId { get; set; }

        public int UserId { get; set; }

        public string UserFullName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public int ReservationStatusId { get; set; }

        public string ReservationStatusName { get; set; } = string.Empty;

        public decimal TotalValue { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<ReservationVehicleResponse> Vehicles { get; set; }
            = new();
    }

    private sealed class ReservationVehicleResponse
    {
        public int ReservationVehicleId { get; set; }

        public int VehicleId { get; set; }

        public string VehicleBrand { get; set; } = string.Empty;

        public string VehicleModel { get; set; } = string.Empty;

        public string LicensePlate { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal DailyPrice { get; set; }

        public int RentalDays { get; set; }

        public decimal RentalValue { get; set; }

        public decimal ExtrasValue { get; set; }

        public decimal TotalValue { get; set; }

        public List<ReservationExtraResponse> Extras { get; set; }
            = new();
    }

    private sealed class ReservationExtraResponse
    {
        public int ExtraId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Total { get; set; }
    }

    private sealed class ApiProblemDetails
    {
        public string? Detail { get; set; }
    }
}

/// <summary>
/// Represents a vehicle submitted when creating a reservation.
/// </summary>
public class CreateReservationVehicleApiModel
{
    public int VehicleId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public ICollection<CreateReservationExtraApiModel> Extras { get; set; }
        = new List<CreateReservationExtraApiModel>();
}

/// <summary>
/// Represents an extra submitted for a reserved vehicle.
/// </summary>
public class CreateReservationExtraApiModel
{
    public int ExtraId { get; set; }

    public int Quantity { get; set; }
}

/// <summary>
/// Represents a reservation consumed by the Web project.
/// </summary>
public class ReservationApiModel
{
    public int ReservationId { get; set; }

    public int UserId { get; set; }

    public string UserFullName { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public int ReservationStatusId { get; set; }

    public string ReservationStatusName { get; set; } = string.Empty;

    public decimal TotalValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<ReservationVehicleApiModel> Vehicles { get; set; }
        = new List<ReservationVehicleApiModel>();
}

/// <summary>
/// Represents a vehicle included in a reservation.
/// </summary>
public class ReservationVehicleApiModel
{
    public int ReservationVehicleId { get; set; }

    public int VehicleId { get; set; }

    public string VehicleBrand { get; set; } = string.Empty;

    public string VehicleModel { get; set; } = string.Empty;

    public string LicensePlate { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal DailyPrice { get; set; }

    public int RentalDays { get; set; }

    public decimal RentalValue { get; set; }

    public decimal ExtrasValue { get; set; }

    public decimal TotalValue { get; set; }

    public ICollection<ReservationExtraApiModel> Extras { get; set; }
        = new List<ReservationExtraApiModel>();
}

/// <summary>
/// Represents an extra included in a reservation.
/// </summary>
public class ReservationExtraApiModel
{
    public int ExtraId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Total { get; set; }
}

/// <summary>
/// Represents the result of a reservation list API request.
/// </summary>
public class ReservationListApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public IReadOnlyList<ReservationApiModel> Reservations { get; set; }
        = Array.Empty<ReservationApiModel>();

    public string? Detail { get; set; }
}

/// <summary>
/// Represents the result of a reservation details API request.
/// </summary>
public class ReservationDetailsApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public ReservationApiModel? Reservation { get; set; }

    public string? Detail { get; set; }
}

/// <summary>
/// Represents the result of a reservation creation API request.
/// </summary>
public class CreateReservationApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public ReservationApiModel? Reservation { get; set; }

    public string? Detail { get; set; }
}

/// <summary>
/// Represents the result of a reservation
/// status action API request.
/// </summary>
public class ReservationActionApiResult
{
    /// <summary>
    /// Gets or sets the HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error detail returned by the API,
    /// when available.
    /// </summary>
    public string? Detail { get; set; }
}
