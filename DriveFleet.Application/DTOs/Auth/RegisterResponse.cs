namespace DriveFleet.Application.DTOs.Auth;

/// <summary>
/// Represents the result of a successful user registration.
/// </summary>
public class RegisterResponse
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public bool EmailConfirmed { get; set; }
}
