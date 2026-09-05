using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the User entity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Maps the entity to the Users table.
        builder.ToTable("Users");

        // Configures the primary key.
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.UserId)
            .ValueGeneratedOnAdd();

        // User's first name is required.
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        // Last name is optional.
        builder.Property(u => u.LastName)
            .HasMaxLength(100);

        // Email is required and must be unique.
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Optional contact phone number.
        builder.Property(u => u.Phone)
            .HasMaxLength(30);

        // Password hash is optional to support external authentication providers.
        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500);

        // Stores the profile photo path or URL.
        builder.Property(u => u.Photo)
            .HasMaxLength(500);

        // Identifies the authentication provider, such as Local or Google.
        builder.Property(u => u.Provider)
            .HasMaxLength(50);

        // New accounts are not considered confirmed by default.
        builder.Property(u => u.EmailConfirmed)
            .HasDefaultValue(false);

        // Audit timestamps.
        builder.Property(u => u.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(u => u.UpdatedAt)
            .HasColumnType("datetime2");

        // Each user belongs to exactly one role.
        // Deleting a role must not automatically delete its users.
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
