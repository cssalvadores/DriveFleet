using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the ReservationVehicleExtra entity.
/// </summary>
public class ReservationVehicleExtraConfiguration
    : IEntityTypeConfiguration<ReservationVehicleExtra>
{
    public void Configure(EntityTypeBuilder<ReservationVehicleExtra> builder)
    {
        // Maps the entity to the ReservationVehicleExtras table and defines
        // database-level integrity constraints.
        builder.ToTable("ReservationVehicleExtras", table =>
        {
            table.HasCheckConstraint(
                "CK_RVE_Quantity",
                "[Quantity] >= 1");

            table.HasCheckConstraint(
                "CK_RVE_Price",
                "[Price] >= 0");
        });

        // Configures the primary key.
        builder.HasKey(rve => rve.ReservationVehicleExtraId);

        builder.Property(rve => rve.ReservationVehicleExtraId)
            .ValueGeneratedOnAdd();

        // Each record belongs to one reserved vehicle.
        builder.Property(rve => rve.ReservationVehicleId)
            .IsRequired();

        // Each record references one extra from the catalogue.
        builder.Property(rve => rve.ExtraId)
            .IsRequired();

        // Quantity must be at least one.
        builder.Property(rve => rve.Quantity)
            .IsRequired();

        // Stores the price applied at the time of the reservation.
        // Future catalogue price changes must not affect historical data.
        builder.Property(rve => rve.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        // Prevents the same extra from being added more than once
        // to the same reserved vehicle.
        builder.HasIndex(rve => new
        {
            rve.ReservationVehicleId,
            rve.ExtraId
        })
        .IsUnique();

        // Each extra assignment belongs to exactly one reserved vehicle.
        // Historical reservation data must be preserved.
        builder.HasOne(rve => rve.ReservationVehicle)
            .WithMany(rv => rv.ReservationVehicleExtras)
            .HasForeignKey(rve => rve.ReservationVehicleId)
            .OnDelete(DeleteBehavior.NoAction);

        // Each extra assignment references exactly one catalogue extra.
        // Deleting an extra must not remove historical reservation data.
        builder.HasOne(rve => rve.Extra)
            .WithMany(e => e.ReservationVehicleExtras)
            .HasForeignKey(rve => rve.ExtraId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
