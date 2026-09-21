using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace DriveFleet.Web.Services;

/// <summary>
/// Provides HTTP operations for profile endpoints
/// exposed by the DriveFleet API.
/// </summary>
public class ProfileApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ProfileApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">
    /// The HTTP client configured to communicate with the DriveFleet API.
    /// </param>
    public ProfileApiClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Retrieves the profile of the authenticated user.
    /// </summary>
    /// <param name="accessToken">
    /// The JWT access token of the authenticated user.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The profile request result returned by the DriveFleet API.
    /// </returns>
    public async Task<UserProfileApiResult> GetProfileAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/profile/me");

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
            var profile =
                await response.Content.ReadFromJsonAsync<UserProfileResponse>(
                    cancellationToken: cancellationToken);

            return new UserProfileApiResult
            {
                StatusCode = response.StatusCode,
                UserId = profile?.UserId,
                FirstName = profile?.FirstName,
                LastName = profile?.LastName,
                Email = profile?.Email,
                Phone = profile?.Phone,
                Photo = profile?.Photo,
                Provider = profile?.Provider,
                EmailConfirmed = profile?.EmailConfirmed,
                Role = profile?.Role
            };
        }

        string? detail = null;

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

        return new UserProfileApiResult
        {
            StatusCode = response.StatusCode,
            Detail = detail
        };
    }

    /// <summary>
    /// Sends an authenticated profile update request
    /// to the DriveFleet API.
    /// </summary>
    /// <param name="accessToken">
    /// The JWT access token of the authenticated user.
    /// </param>
    /// <param name="firstName">
    /// The user's first name.
    /// </param>
    /// <param name="lastName">
    /// The user's last name.
    /// </param>
    /// <param name="phone">
    /// The user's phone number.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The result returned by the DriveFleet API.
    /// </returns>
    public async Task<UpdateProfileApiResult> UpdateProfileAsync(
        string accessToken,
        string firstName,
        string? lastName,
        string? phone,
        CancellationToken cancellationToken = default)
    {
        var request = new UpdateProfileRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Phone = phone
        };

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Put,
                "api/profile/me");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        httpRequest.Content =
            JsonContent.Create(request);

        // Sends the authenticated profile update request to the API.
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

        return new UpdateProfileApiResult
        {
            StatusCode = response.StatusCode,
            Detail = detail
        };
    }

    /// <summary>
    /// Uploads a new profile photo for the authenticated user.
    /// </summary>
    /// <param name="accessToken">
    /// The JWT access token of the authenticated user.
    /// </param>
    /// <param name="photoStream">
    /// The stream containing the profile photo.
    /// </param>
    /// <param name="fileName">
    /// The original name of the uploaded file.
    /// </param>
    /// <param name="contentType">
    /// The MIME content type of the uploaded file.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous HTTP request if needed.
    /// </param>
    /// <returns>
    /// The result returned by the DriveFleet API.
    /// </returns>
    public async Task<UploadProfilePhotoApiResult> UploadProfilePhotoAsync(
        string accessToken,
        Stream photoStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/profile/me/photo");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        using var formContent =
            new MultipartFormDataContent();

        using var fileContent =
            new StreamContent(photoStream);

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(contentType);
        }

        formContent.Add(
            fileContent,
            "photo",
            fileName);

        request.Content = formContent;

        // Sends the profile photo to the API as multipart/form-data.
        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var photoResponse =
                await response.Content.ReadFromJsonAsync<ProfilePhotoResponse>(
                    cancellationToken: cancellationToken);

            return new UploadProfilePhotoApiResult
            {
                StatusCode = response.StatusCode,
                Photo = photoResponse?.Photo
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

        return new UploadProfilePhotoApiResult
        {
            StatusCode = response.StatusCode,
            Detail = detail
        };
    }

    /// <summary>
    /// Represents a profile response returned by the API.
    /// </summary>
    private sealed class UserProfileResponse
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Photo { get; set; }

        public string? Provider { get; set; }

        public bool EmailConfirmed { get; set; }

        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents ProblemDetails information returned by the API.
    /// </summary>
    private sealed class ApiProblemDetails
    {
        public string? Detail { get; set; }
    }

    /// <summary>
    /// Represents a profile update request sent to the API.
    /// </summary>
    private sealed class UpdateProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string? Phone { get; set; }
    }

    /// <summary>
    /// Represents a successful profile photo response
    /// returned by the API.
    /// </summary>
    private sealed class ProfilePhotoResponse
    {
        public string Photo { get; set; } = string.Empty;
    }
}



/// <summary>
/// Represents the result of a profile API request.
/// </summary>
public class UserProfileApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public int? UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Photo { get; set; }

    public string? Provider { get; set; }

    public bool? EmailConfirmed { get; set; }

    public string? Role { get; set; }

    public string? Detail { get; set; }
}

/// <summary>
/// Represents the result of a profile update API request.
/// </summary>
public class UpdateProfileApiResult
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
/// Represents the result of a profile photo upload API request.
/// </summary>
public class UploadProfilePhotoApiResult
{
    /// <summary>
    /// Gets or sets the HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the relative path of the uploaded profile photo.
    /// </summary>
    public string? Photo { get; set; }

    /// <summary>
    /// Gets or sets the error detail returned by the API, when available.
    /// </summary>
    public string? Detail { get; set; }
}

