using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the Reservation entity.
/// </summary>
public class ReservationConfiguration
    : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        // Maps the entity to the Reservations table and defines
        // database-level integrity constraints.
        builder.ToTable("Reservations", table =>
        {
            table.HasCheckConstraint(
                "CK_Reservations_TotalValue",
                "[TotalValue] >= 0");
        });

        // Configures the primary key.
        builder.HasKey(r => r.ReservationId);

        builder.Property(r => r.ReservationId)
            .ValueGeneratedOnAdd();

        // Each reservation must belong to a user.
        builder.Property(r => r.UserId)
            .IsRequired();

        // Each reservation must have a lifecycle status.
        builder.Property(r => r.ReservationStatusId)
            .IsRequired();

        // Stores the final amount calculated by the backend.
        builder.Property(r => r.TotalValue)
            .IsRequired()
            .HasPrecision(10, 2);

        // Audit timestamps.
        builder.Property(r => r.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(r => r.UpdatedAt)
            .HasColumnType("datetime2");

        // Each reservation belongs to exactly one user.
        // Historical reservations must not be deleted through cascade deletion.
        builder.HasOne(r => r.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Each reservation has exactly one lifecycle status.
        // Deleting a status must not delete historical reservations.
        builder.HasOne(r => r.ReservationStatus)
            .WithMany(rs => rs.Reservations)
            .HasForeignKey(r => r.ReservationStatusId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
