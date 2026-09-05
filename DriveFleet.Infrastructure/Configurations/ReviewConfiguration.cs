using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the Review entity.
/// </summary>
public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        // Maps the entity to the Reviews table and defines
        // database-level integrity constraints.
        builder.ToTable("Reviews", table =>
        {
            table.HasCheckConstraint(
                "CK_Reviews_Stars",
                "[Stars] >= 1 AND [Stars] <= 5");
        });

        // Configures the primary key.
        builder.HasKey(r => r.ReviewId);

        builder.Property(r => r.ReviewId)
            .ValueGeneratedOnAdd();

        // Each review belongs to one specific reserved vehicle.
        builder.Property(r => r.ReservationVehicleId)
            .IsRequired();

        // Ratings use a value between 1 and 5.
        builder.Property(r => r.Stars)
            .IsRequired()
            .HasColumnType("tinyint");

        // The written comment is optional.
        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        // Reviews are visible by default.
        // Moderation can hide them without deleting historical data.
        builder.Property(r => r.IsVisible)
            .HasDefaultValue(true);

        // Audit timestamps.
        builder.Property(r => r.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(r => r.UpdatedAt)
            .HasColumnType("datetime2");

        // Guarantees that each reserved vehicle can have at most one review.
        builder.HasIndex(r => r.ReservationVehicleId)
            .IsUnique();

        // A review belongs to exactly one reserved vehicle.
        // Deleting historical reservation data must not delete reviews automatically.
        builder.HasOne(r => r.ReservationVehicle)
            .WithOne(rv => rv.Review)
            .HasForeignKey<Review>(r => r.ReservationVehicleId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
