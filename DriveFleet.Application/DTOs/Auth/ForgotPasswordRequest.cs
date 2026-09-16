using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Application.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
}
