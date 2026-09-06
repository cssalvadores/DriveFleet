using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Application.DTOs.Auth;

/// <summary>
/// Represents the data required to register a new client account.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    [StringLength(100)]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's phone number.
    /// </summary>
    [StringLength(30)]
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the user's password.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

