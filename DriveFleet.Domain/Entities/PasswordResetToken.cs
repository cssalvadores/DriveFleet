using System.Collections.Generic;

namespace DriveFleet.Domain.Entities;

public class PasswordResetToken
{
    public int PasswordResetTokenId { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
