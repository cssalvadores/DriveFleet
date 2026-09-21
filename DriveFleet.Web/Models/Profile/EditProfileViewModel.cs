using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Web.Models.Profile;

/// <summary>
/// Represents the profile editing form
/// in the DriveFleet web application.
/// </summary>
public class EditProfileViewModel
{
    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    [Required]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    [Display(Name = "Last name")]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the user's phone number.
    /// </summary>
    [Phone]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }
}


