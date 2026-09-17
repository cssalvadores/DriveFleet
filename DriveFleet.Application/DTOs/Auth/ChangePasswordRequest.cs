using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Application.DTOs.Auth;

/// <summary>
/// Represents a request from an authenticated user
/// to change the account password.
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// Gets or sets the user's current password.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confirmation of the new password.
    /// </summary>
    [Required]
    [Compare(
        nameof(NewPassword),
        ErrorMessage = "The password confirmation does not match the new password.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
