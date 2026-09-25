using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Web.Models.Auth;

/// <summary>
/// Represents the registration form displayed to a new client.
/// </summary>
public class RegisterViewModel
{
    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    [Required]
    [StringLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    [StringLength(100)]
    [Display(Name = "Last name")]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's phone number.
    /// </summary>
    [StringLength(30)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the user's password.
    /// </summary>
    [Required]
    [StringLength(
        100,
        MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password confirmation entered by the user.
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    [Compare(
        nameof(Password),
        ErrorMessage = "The password and confirmation password do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
