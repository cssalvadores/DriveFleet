using System.Net.Http.Json;
using System.Text.Json;
using DriveFleet.Mobile.Models.Auth;

namespace DriveFleet.Mobile.Services;

/// <summary>
/// Provides authentication operations against
/// the DriveFleet REST API.
/// </summary>
public class AuthApiService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AuthApiService"/> class.
    /// </summary>
    /// <param name="httpClient">
    /// HTTP client configured for the DriveFleet API.
    /// </param>
    public AuthApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Authenticates a user using the supplied
    /// email address and password.
    /// </summary>
    /// <param name="email">
    /// The employee email address.
    /// </param>
    /// <param name="password">
    /// The employee password.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the HTTP request.
    /// </param>
    /// <returns>
    /// The authentication result returned by the API.
    /// </returns>
    public async Task<LoginApiResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var request =
            new LoginRequest
            {
                Email = email,
                Password = password
            };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/auth/login",
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var login =
                await response.Content
                    .ReadFromJsonAsync<LoginResponse>(
                        cancellationToken:
                            cancellationToken);

            return new LoginApiResult
            {
                StatusCode =
                    response.StatusCode,

                Login =
                    login
            };
        }

        return new LoginApiResult
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
    /// Reads a ProblemDetails response returned
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
    /// Represents the relevant ProblemDetails information
    /// returned by the API.
    /// </summary>
    private sealed class ApiProblemDetails
    {
        public string? Detail { get; set; }
    }
}
