using System.Collections.Generic;

namespace DriveFleet.Domain.Entities;

/// <summary>
/// Represents a token used to confirm a user's email address.
/// Only the token hash is persisted for security reasons.
/// </summary>
public class EmailConfirmationToken
{
    public int EmailConfirmationTokenId { get; set; }

    public int UserId { get; set; }

    // Stores only the hash of the confirmation token.
    // The raw token is never persisted in the database.
    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
