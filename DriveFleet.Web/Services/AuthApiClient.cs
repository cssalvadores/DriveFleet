using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity.Data;
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
    /// Sends user credentials to the DriveFleet API
    /// and returns the authentication result.
    /// </summary>
    /// <param name="email">
    /// The user's email address.
    /// </param>
    /// <param name="password">
    /// The user's password.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The authentication result returned by the DriveFleet API.
    /// </returns>
    public async Task<LoginApiResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "api/auth/login",
            request,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var loginResponse =
                await response.Content.ReadFromJsonAsync<LoginResponse>(
                    cancellationToken: cancellationToken);

            return new LoginApiResult
            {
                StatusCode = response.StatusCode,
                UserId = loginResponse?.UserId,
                FirstName = loginResponse?.FirstName,
                LastName = loginResponse?.LastName,
                Email = loginResponse?.Email,
                Role = loginResponse?.Role,
                AccessToken = loginResponse?.AccessToken,
                ExpiresAt = loginResponse?.ExpiresAt
            };
        }

        string? detail = null;

        // Reads ProblemDetails information when the API returns an error.
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

        return new LoginApiResult
        {
            StatusCode = response.StatusCode,
            Detail = detail
        };
    }

    /// <summary>
    /// Sends registration information to the DriveFleet API.
    /// </summary>
    /// <param name="firstName">
    /// The user's first name.
    /// </param>
    /// <param name="lastName">
    /// The user's last name.
    /// </param>
    /// <param name="email">
    /// The user's email address.
    /// </param>
    /// <param name="phone">
    /// The user's phone number.
    /// </param>
    /// <param name="password">
    /// The user's password.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The registration result returned by the DriveFleet API.
    /// </returns>
    public async Task<RegisterApiResult> RegisterAsync(
        string firstName,
        string? lastName,
        string email,
        string? phone,
        string password,
        CancellationToken cancellationToken = default)
    {
        var request = new RegisterRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Password = password
        };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/auth/register",
                request,
                cancellationToken);

        string? detail = null;

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

        return new RegisterApiResult
        {
            StatusCode = response.StatusCode,
            Detail = detail
        };
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
    /// Sends a password reset request to the DriveFleet API.
    /// </summary>
    /// <param name="token">
    /// The raw password reset token received through the reset link.
    /// </param>
    /// <param name="newPassword">
    /// The new password selected by the user.
    /// </param>
    /// <param name="confirmPassword">
    /// The confirmation of the new password.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The result returned by the DriveFleet API.
    /// </returns>
    public async Task<ResetPasswordApiResult> ResetPasswordAsync(
        string token,
        string newPassword,
        string confirmPassword,
        CancellationToken cancellationToken = default)
    {
        var request = new ResetPasswordRequest
        {
            Token = token,
            NewPassword = newPassword,
            ConfirmPassword = confirmPassword
        };

        // Sends the password reset information to the API.
        using var response = await _httpClient.PostAsJsonAsync(
            "api/auth/reset-password",
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

        return new ResetPasswordApiResult
        {
            StatusCode = response.StatusCode,
            Detail = detail
        };
    }

    /// <summary>
    /// Represents the result of a password change API request.
    /// </summary>
    public class ChangePasswordApiResult
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

    /// <summary>
    /// Sends an authenticated password change request
    /// to the DriveFleet API.
    /// </summary>
    /// <param name="accessToken">
    /// The JWT access token of the authenticated user.
    /// </param>
    /// <param name="currentPassword">
    /// The user's current password.
    /// </param>
    /// <param name="newPassword">
    /// The new password selected by the user.
    /// </param>
    /// <param name="confirmNewPassword">
    /// The confirmation of the new password.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The result returned by the DriveFleet API.
    /// </returns>
    public async Task<ChangePasswordApiResult> ChangePasswordAsync(
        string accessToken,
        string currentPassword,
        string newPassword,
        string confirmNewPassword,
        CancellationToken cancellationToken = default)
    {
        var request = new ChangePasswordRequest
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword,
            ConfirmNewPassword = confirmNewPassword
        };

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/auth/change-password");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        httpRequest.Content =
            JsonContent.Create(request);

        // Sends the authenticated password change request to the API.
        using var response =
            await _httpClient.SendAsync(
                httpRequest,
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

        return new ChangePasswordApiResult
        {
            StatusCode = response.StatusCode,
            Detail = detail
        };
    }

    /// <summary>
    /// Sends an authenticated logout request to the DriveFleet API.
    /// </summary>
    /// <param name="accessToken">
    /// The JWT access token of the authenticated user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The HTTP status code returned by the DriveFleet API.
    /// </returns>
    public async Task<HttpStatusCode> LogoutAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/auth/logout");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        // Sends the authenticated logout request to the API.
        using var response =
            await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

        return response.StatusCode;
    }

    /// <summary>
    /// Represents the registration information sent to the API.
    /// </summary>
    private sealed class RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the credentials sent to the API
    /// when authenticating a user.
    /// </summary>
    private sealed class LoginRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a successful login response returned by the API.
    /// </summary>
    private sealed class LoginResponse
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
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
    /// Represents the request sent to the API
    /// when resetting a user's password.
    /// </summary>
    private sealed class ResetPasswordRequest
    {
        public string Token { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the request sent to the API
    /// when changing an authenticated user's password.
    /// </summary>
    private sealed class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;

        public string ConfirmNewPassword { get; set; } = string.Empty;
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
/// Represents the result of a login API request.
/// </summary>
public class LoginApiResult
{
    /// <summary>
    /// Gets or sets the HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user's identifier.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user's email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user's role.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Gets or sets the JWT access token returned by the API.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the access token expires.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the error detail returned by the API, when available.
    /// </summary>
    public string? Detail { get; set; }
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

/// <summary>
/// Represents the result of a password reset API request.
/// </summary>
public class ResetPasswordApiResult
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

/// <summary>
/// Represents the result of a registration API request.
/// </summary>
public class RegisterApiResult
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
