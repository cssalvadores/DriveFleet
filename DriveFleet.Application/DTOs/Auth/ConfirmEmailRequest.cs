using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Application.DTOs.Auth;

/// <summary>
/// Represents the data required to confirm a user's email address.
/// </summary>
public class ConfirmEmailRequest
{
    /// <summary>
    /// Gets or sets the raw email confirmation token.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;
}
