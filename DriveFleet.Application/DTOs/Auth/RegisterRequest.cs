namespace DriveFleet.Application.DTOs.Auth;

/// <summary>
/// Represents the data required to register a new client account.
/// </summary>
public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string Password { get; set; } = string.Empty;
}
