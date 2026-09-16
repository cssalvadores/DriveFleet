using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Web.Models.Auth;

/// <summary>
/// Represents the password reset form displayed to the user.
/// </summary>
public class ResetPasswordViewModel
{
    /// <summary>
    /// Gets or sets the password reset token received by email.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;

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
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
