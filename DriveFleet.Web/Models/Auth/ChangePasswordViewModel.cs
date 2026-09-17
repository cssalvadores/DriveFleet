using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Web.Models.Auth;

/// <summary>
/// Represents the authenticated password change form.
/// </summary>
public class ChangePasswordViewModel
{
    /// <summary>
    /// Gets or sets the user's current password.
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    [StringLength(
        100,
        MinimumLength = 8,
        ErrorMessage = "The password must contain between 8 and 100 characters.")]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confirmation of the new password.
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    [Compare(
        nameof(NewPassword),
        ErrorMessage = "The password confirmation does not match.")]
    [Display(Name = "Confirm new password")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
