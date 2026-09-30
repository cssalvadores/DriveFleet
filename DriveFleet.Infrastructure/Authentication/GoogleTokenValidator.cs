using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DriveFleet.Application.DTOs.Auth;
using DriveFleet.Application.Interfaces;
using DriveFleet.Application.Options;
using Microsoft.Extensions.Options;

namespace DriveFleet.Infrastructure.Authentication;

/// <summary>
/// Validates Google OAuth access tokens
/// against Google services.
/// </summary>
public sealed class GoogleTokenValidator
    : IGoogleTokenValidator
{
    private readonly HttpClient _httpClient;

    private readonly GoogleAuthenticationSettings
        _googleAuthenticationSettings;

    public GoogleTokenValidator(
        HttpClient httpClient,
        IOptions<GoogleAuthenticationSettings>
            googleAuthenticationOptions)
    {
        _httpClient =
            httpClient;

        _googleAuthenticationSettings =
            googleAuthenticationOptions.Value;

        if (string.IsNullOrWhiteSpace(
            _googleAuthenticationSettings.ClientId))
        {
            throw new InvalidOperationException(
                "The Google OAuth Client ID is not configured.");
        }
    }

    /// <inheritdoc />
    public async Task<GoogleIdentity?> ValidateAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        /*
         * First verifies that the access token was issued
         * for the DriveFleet Google OAuth client.
         */
        using var tokenInfoResponse =
            await _httpClient.GetAsync(
                "oauth2/v3/tokeninfo?access_token=" +
                Uri.EscapeDataString(accessToken),
                cancellationToken);

        if (!tokenInfoResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var tokenInfo =
            await tokenInfoResponse.Content
                .ReadFromJsonAsync<GoogleTokenInfoResponse>(
                    cancellationToken:
                        cancellationToken);

        if (tokenInfo is null ||
            string.IsNullOrWhiteSpace(
                tokenInfo.Audience) ||
            !string.Equals(
                tokenInfo.Audience,
                _googleAuthenticationSettings.ClientId,
                StringComparison.Ordinal))
        {
            return null;
        }

        /*
         * After validating the token audience,
         * retrieves the authenticated user's profile.
         */
        using var userInfoRequest =
            new HttpRequestMessage(
                HttpMethod.Get,
                "oauth2/v2/userinfo");

        userInfoRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        using var userInfoResponse =
            await _httpClient.SendAsync(
                userInfoRequest,
                cancellationToken);

        if (!userInfoResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var userInfo =
            await userInfoResponse.Content
                .ReadFromJsonAsync<GoogleUserInfoResponse>(
                    cancellationToken:
                        cancellationToken);

        if (userInfo is null ||
            string.IsNullOrWhiteSpace(userInfo.Id) ||
            string.IsNullOrWhiteSpace(userInfo.Email) ||
            !userInfo.VerifiedEmail)
        {
            return null;
        }

        return new GoogleIdentity
        {
            Subject =
                userInfo.Id,

            Email =
                userInfo.Email,

            FirstName =
                userInfo.GivenName,

            LastName =
                userInfo.FamilyName,

            HostedDomain =
                userInfo.HostedDomain
        };
    }

    private sealed class GoogleTokenInfoResponse
    {
        [JsonPropertyName("aud")]
        public string? Audience { get; set; }
    }

    private sealed class GoogleUserInfoResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("verified_email")]
        public bool VerifiedEmail { get; set; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }

        [JsonPropertyName("hd")]
        public string? HostedDomain { get; set; }
    }
}
