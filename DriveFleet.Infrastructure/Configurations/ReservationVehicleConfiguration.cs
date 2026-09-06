using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the ReservationVehicle entity.
/// </summary>
public class ReservationVehicleConfiguration
    : IEntityTypeConfiguration<ReservationVehicle>
{
    public void Configure(EntityTypeBuilder<ReservationVehicle> builder)
    {
        // Maps the entity to the ReservationVehicles table and defines
        // database-level integrity constraints.
        builder.ToTable("ReservationVehicles", table =>
        {
            // Each reserved vehicle must have a valid rental period.
            table.HasCheckConstraint(
                "CK_ReservationVehicles_Dates",
                "[EndDate] > [StartDate]");

            // The contracted daily price cannot be negative.
            table.HasCheckConstraint(
                "CK_ReservationVehicles_DailyPrice",
                "[DailyPrice] >= 0");
        });

        // Configures the primary key.
        builder.HasKey(rv => rv.ReservationVehicleId);

        builder.Property(rv => rv.ReservationVehicleId)
            .ValueGeneratedOnAdd();

        // Each record belongs to exactly one reservation.
        builder.Property(rv => rv.ReservationId)
            .IsRequired();

        // Each record references exactly one vehicle.
        builder.Property(rv => rv.VehicleId)
            .IsRequired();

        // Rental period for this specific vehicle.
        builder.Property(rv => rv.StartDate)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(rv => rv.EndDate)
            .IsRequired()
            .HasColumnType("datetime2");

        // Stores the daily price agreed at the time of the reservation.
        // Future catalogue price changes must not affect historical reservations.
        builder.Property(rv => rv.DailyPrice)
            .IsRequired()
            .HasPrecision(10, 2);

        // Prevents the same vehicle from being added more than once
        // to the same reservation.
        builder.HasIndex(rv => new
        {
            rv.ReservationId,
            rv.VehicleId
        })
        .IsUnique();

        // Optimizes vehicle availability queries by vehicle and rental period.
        builder.HasIndex(rv => new
        {
            rv.VehicleId,
            rv.StartDate,
            rv.EndDate
        });

        // Each reserved vehicle belongs to exactly one reservation.
        // Reservation history must be preserved.
        builder.HasOne(rv => rv.Reservation)
            .WithMany(r => r.ReservationVehicles)
            .HasForeignKey(rv => rv.ReservationId)
            .OnDelete(DeleteBehavior.NoAction);

        // Each reservation vehicle references exactly one fleet vehicle.
        // Deleting a vehicle must not remove historical reservation data.
        builder.HasOne(rv => rv.Vehicle)
            .WithMany(v => v.ReservationVehicles)
            .HasForeignKey(rv => rv.VehicleId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
