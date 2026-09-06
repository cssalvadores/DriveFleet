using System.Collections.Generic;

namespace DriveFleet.Domain.Entities;

/// <summary>
/// Represents a temporary token used to reset a user's password.
/// Only the token hash is persisted for security reasons.
/// </summary>
public class PasswordResetToken
{
    public int PasswordResetTokenId { get; set; }

    public int UserId { get; set; }

    // Stores only the hash of the password reset token.
    // The raw token is never persisted in the database.
    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
