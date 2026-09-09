using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DriveFleet.Web.Services;

/// <summary>
/// Provides HTTP operations for authentication endpoints
/// exposed by the DriveFleet API.
/// </summary>
public class AuthApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AuthApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">
    /// The HTTP client configured to communicate with the DriveFleet API.
    /// </param>
    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Sends an email confirmation token to the DriveFleet API.
    /// </summary>
    /// <param name="token">
    /// The raw email confirmation token received through the confirmation link.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The result returned by the DriveFleet API.
    /// </returns>
    public async Task<ConfirmEmailApiResult> ConfirmEmailAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var request = new ConfirmEmailRequest
        {
            Token = token
        };

        // Sends the raw confirmation token to the API.
        using var response = await _httpClient.PostAsJsonAsync(
            "api/auth/confirm-email",
            request,
            cancellationToken);

        string? detail = null;

        // Reads ProblemDetails information when the API returns an error.
        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var problemDetails =
                    await response.Content.ReadFromJsonAsync<ApiProblemDetails>(
                        cancellationToken: cancellationToken);

                detail = problemDetails?.Detail;
            }
            catch (JsonException)
            {
                // Keeps the detail empty if the response body is not valid JSON.
            }
        }

        return new ConfirmEmailApiResult
        {
            StatusCode = response.StatusCode,
            Detail = detail
        };
    }

    /// <summary>
    /// Represents the request sent to the API
    /// when confirming an email address.
    /// </summary>
    private sealed class ConfirmEmailRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents ProblemDetails information returned by the API.
    /// </summary>
    private sealed class ApiProblemDetails
    {
        public string? Detail { get; set; }
    }
}

/// <summary>
/// Represents the result of an email confirmation API request.
/// </summary>
public class ConfirmEmailApiResult
{
    /// <summary>
    /// Gets or sets the HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error detail returned by the API, when available.
    /// </summary>
    public string? Detail { get; set; }
}
