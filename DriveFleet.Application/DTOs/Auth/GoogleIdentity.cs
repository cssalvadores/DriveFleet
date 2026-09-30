namespace DriveFleet.Application.DTOs.Auth;

/// <summary>
/// Represents identity information validated
/// directly with Google.
/// </summary>
public class GoogleIdentity
{
    public string Subject { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? HostedDomain { get; set; }
}
