using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DriveFleet.Web.Models.Profile;

/// <summary>
/// Represents the profile photo upload form
/// in the DriveFleet web application.
/// </summary>
public class ProfilePhotoViewModel
{
    /// <summary>
    /// Gets or sets the path of the user's current profile photo.
    /// </summary>
    public string? CurrentPhoto { get; set; }

    /// <summary>
    /// Gets or sets the new profile photo selected by the user.
    /// </summary>
    [Required(ErrorMessage = "Please select a profile photo.")]
    [Display(Name = "Profile photo")]
    public IFormFile? Photo { get; set; }
}
