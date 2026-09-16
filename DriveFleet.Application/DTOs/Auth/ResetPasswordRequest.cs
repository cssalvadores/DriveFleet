using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Application.DTOs.Auth;

/// <summary>
/// Represents a request to reset a user's password
/// using a password reset token.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Gets or sets the raw password reset token.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password confirmation.
    /// </summary>
    [Required]
    [Compare(
        nameof(NewPassword),
        ErrorMessage = "The password confirmation does not match the new password.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
