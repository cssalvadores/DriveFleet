using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the PasswordResetToken entity.
/// </summary>
public class PasswordResetTokenConfiguration
    : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        // Maps the entity to the PasswordResetTokens table.
        builder.ToTable("PasswordResetTokens");

        // Configures the primary key.
        builder.HasKey(t => t.PasswordResetTokenId);

        builder.Property(t => t.PasswordResetTokenId)
            .ValueGeneratedOnAdd();

        // Each token belongs to exactly one user.
        builder.Property(t => t.UserId)
            .IsRequired();

        // Reset tokens are required and must be unique.
        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(t => t.Token)
            .IsUnique();

        // Defines when the token stops being valid.
        builder.Property(t => t.ExpiresAt)
            .IsRequired()
            .HasColumnType("datetime2");

        // Remains null until the token is successfully used.
        builder.Property(t => t.UsedAt)
            .HasColumnType("datetime2");

        // Records when the token was created.
        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        // Reset tokens may be deleted automatically with their owning user.
        builder.HasOne(t => t.User)
            .WithMany(u => u.PasswordResetTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
