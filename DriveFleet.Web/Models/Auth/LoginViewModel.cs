using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Web.Models.Auth;

/// <summary>
/// Represents the login form for the DriveFleet web application.
/// </summary>
public class LoginViewModel
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password.
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the authentication cookie
    /// should persist after the browser session ends.
    /// </summary>
    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
