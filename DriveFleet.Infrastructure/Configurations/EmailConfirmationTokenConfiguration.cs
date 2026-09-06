using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the EmailConfirmationToken entity.
/// </summary>
public class EmailConfirmationTokenConfiguration
    : IEntityTypeConfiguration<EmailConfirmationToken>
{
    public void Configure(EntityTypeBuilder<EmailConfirmationToken> builder)
    {
        // Maps the entity to the EmailConfirmationTokens table.
        builder.ToTable("EmailConfirmationTokens");

        // Configures the primary key.
        builder.HasKey(t => t.EmailConfirmationTokenId);

        builder.Property(t => t.EmailConfirmationTokenId)
            .ValueGeneratedOnAdd();

        // Each token belongs to exactly one user.
        builder.Property(t => t.UserId)
            .IsRequired();

        // Only the token hash is persisted.
        // The raw confirmation token is never stored.
        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(t => t.TokenHash)
            .IsUnique();

        // Defines when the token stops being valid.
        builder.Property(t => t.ExpiresAt)
            .IsRequired()
            .HasColumnType("datetime2");

        // Remains null until the token is successfully used.
        builder.Property(t => t.ConfirmedAt)
            .HasColumnType("datetime2");

        // Records when the token was created.
        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        // Tokens are dependent authentication data.
        // They may be removed automatically when the owning user is deleted.
        builder.HasOne(t => t.User)
            .WithMany(u => u.EmailConfirmationTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
