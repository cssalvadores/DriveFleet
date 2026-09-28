using System.Net;

namespace DriveFleet.Mobile.Models.Auth;

/// <summary>
/// Represents the result of a login request
/// made by the mobile application.
/// </summary>
public class LoginApiResult
{
    /// <summary>
    /// Gets or sets the HTTP status code
    /// returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user information
    /// when authentication succeeds.
    /// </summary>
    public LoginResponse? Login { get; set; }

    /// <summary>
    /// Gets or sets the API error detail
    /// when authentication fails.
    /// </summary>
    public string? Detail { get; set; }
}
