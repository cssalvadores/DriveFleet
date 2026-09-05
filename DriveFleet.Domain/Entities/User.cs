using System.Collections.Generic;

namespace DriveFleet.Domain.Entities;

public class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? PasswordHash { get; set; }

    public string? Photo { get; set; }

    public string? Provider { get; set; }

    public bool EmailConfirmed { get; set; }

    public int RoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Role Role { get; set; } = null!;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public ICollection<EmailConfirmationToken> EmailConfirmationTokens { get; set; }
        = new List<EmailConfirmationToken>();

    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; }
        = new List<PasswordResetToken>();
}
