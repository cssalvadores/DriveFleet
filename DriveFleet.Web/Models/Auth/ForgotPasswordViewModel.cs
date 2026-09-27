using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Web.Models.Auth;

/// <summary>
/// Represents the information required to request
/// a password reset link.
/// </summary>
public class ForgotPasswordViewModel
{
    /// <summary>
    /// Gets or sets the email address associated
    /// with the user account.
    /// </summary>
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}
