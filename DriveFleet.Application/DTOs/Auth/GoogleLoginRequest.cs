namespace DriveFleet.Application.DTOs.Auth;

/// <summary>
/// Represents a Google authentication request.
/// </summary>
public class GoogleLoginRequest
{   
    
    /// <summary>
    /// Gets or sets the Google OAuth access token
    /// obtained after successful Google authentication.
    /// </summary>
    public string AccessToken { get; set; } =
        string.Empty;
}
