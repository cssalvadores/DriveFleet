using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the ReservationStatus entity.
/// </summary>
public class ReservationStatusConfiguration
    : IEntityTypeConfiguration<ReservationStatus>
{
    public void Configure(EntityTypeBuilder<ReservationStatus> builder)
    {
        // Maps the entity to the ReservationStatus table.
        builder.ToTable("ReservationStatus");

        // Configures the primary key.
        builder.HasKey(rs => rs.ReservationStatusId);

        builder.Property(rs => rs.ReservationStatusId)
            .ValueGeneratedOnAdd();

        // Reservation status names are required and must be unique.
        builder.Property(rs => rs.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(rs => rs.Name)
            .IsUnique();
    }
}
