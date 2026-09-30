namespace DriveFleet.Application.Options;

/// <summary>
/// Contains Google authentication configuration.
/// </summary>
public class GoogleAuthenticationSettings
{
    public const string SectionName =
        "Authentication:Google";

    public string ClientId { get; set; } =
        string.Empty;
}


