using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Application.DTOs.Auth;

/// <summary>
/// Represents the credentials required to authenticate a user.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;
}
