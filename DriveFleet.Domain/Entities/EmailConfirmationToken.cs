using System.Collections.Generic;

namespace DriveFleet.Domain.Entities;

public class EmailConfirmationToken
{
    public int EmailConfirmationTokenId { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
