using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace DriveFleet.Web.Services;

/// <summary>
/// Provides HTTP operations for administrative
/// review moderation endpoints exposed by the DriveFleet API.
/// </summary>
public class AdminReviewApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AdminReviewApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">
    /// The HTTP client configured to communicate
    /// with the DriveFleet API.
    /// </param>
    public AdminReviewApiClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Retrieves all reviews available for
    /// administrative moderation.
    /// </summary>
    /// <param name="accessToken">
    /// The JWT access token of the authenticated administrator.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request.
    /// </param>
    /// <returns>
    /// The review list request result returned by the API.
    /// </returns>
    public async Task<AdminReviewListApiResult> GetAllAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/reviews");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.OK)
        {
            var reviews =
                await response.Content
                    .ReadFromJsonAsync<List<AdminReviewApiModel>>(
                        cancellationToken:
                            cancellationToken);

            return new AdminReviewListApiResult
            {
                StatusCode =
                    response.StatusCode,

                Reviews =
                    reviews ??
                    new List<AdminReviewApiModel>()
            };
        }

        return new AdminReviewListApiResult
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
    /// Updates whether a review is publicly visible.
    /// </summary>
    /// <param name="accessToken">
    /// The JWT access token of the authenticated administrator.
    /// </param>
    /// <param name="reviewId">
    /// The identifier of the review to update.
    /// </param>
    /// <param name="isVisible">
    /// The new public visibility state.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request.
    /// </param>
    /// <returns>
    /// The review visibility update result returned by the API.
    /// </returns>
    public async Task<AdminReviewActionApiResult>
        SetVisibilityAsync(
            string accessToken,
            int reviewId,
            bool isVisible,
            CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"api/reviews/{reviewId}/visibility");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        request.Content =
            JsonContent.Create(
                new UpdateReviewVisibilityApiRequest
                {
                    IsVisible =
                        isVisible
                });

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        AdminReviewApiModel? review =
            null;

        if (response.StatusCode ==
            HttpStatusCode.OK)
        {
            review =
                await response.Content
                    .ReadFromJsonAsync<AdminReviewApiModel>(
                        cancellationToken:
                            cancellationToken);
        }

        return new AdminReviewActionApiResult
        {
            StatusCode =
                response.StatusCode,

            Review =
                review,

            Detail =
                response.IsSuccessStatusCode
                    ? null
                    : await ReadProblemDetailAsync(
                        response,
                        cancellationToken)
        };
    }

    /// <summary>
    /// Permanently deletes a review.
    /// </summary>
    /// <param name="accessToken">
    /// The JWT access token of the authenticated administrator.
    /// </param>
    /// <param name="reviewId">
    /// The identifier of the review to delete.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request.
    /// </param>
    /// <returns>
    /// The HTTP result returned by the API.
    /// </returns>
    public async Task<AdminReviewActionApiResult> DeleteAsync(
        string accessToken,
        int reviewId,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"api/reviews/{reviewId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        return new AdminReviewActionApiResult
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
                        cancellationToken:
                            cancellationToken);

            return problemDetails?.Detail;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Represents the review visibility request
    /// sent to the API.
    /// </summary>
    private sealed class UpdateReviewVisibilityApiRequest
    {
        public bool IsVisible { get; set; }
    }

    /// <summary>
    /// Represents ProblemDetails information
    /// returned by the API.
    /// </summary>
    private sealed class ApiProblemDetails
    {
        public string? Detail { get; set; }
    }
}

/// <summary>
/// Represents a review consumed by the
/// administrative Web interface.
/// </summary>
public class AdminReviewApiModel
{
    public int ReviewId { get; set; }

    public int ReservationVehicleId { get; set; }

    public int VehicleId { get; set; }

    public int Stars { get; set; }

    public string? Comment { get; set; }

    public bool IsVisible { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Represents the result of an administrative
/// review list API request.
/// </summary>
public class AdminReviewListApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public IReadOnlyList<AdminReviewApiModel> Reviews { get; set; }
        = Array.Empty<AdminReviewApiModel>();

    public string? Detail { get; set; }
}

/// <summary>
/// Represents the result of an administrative
/// review modification API request.
/// </summary>
public class AdminReviewActionApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public AdminReviewApiModel? Review { get; set; }

    public string? Detail { get; set; }
}
