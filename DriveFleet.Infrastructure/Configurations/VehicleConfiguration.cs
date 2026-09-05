using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the Vehicle entity.
/// </summary>
public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        // Maps the entity to the Vehicles table and defines
        // database-level integrity constraints.
        builder.ToTable("Vehicles", table =>
        {
            table.HasCheckConstraint(
                "CK_Vehicles_Seats",
                "[Seats] > 0");

            table.HasCheckConstraint(
                "CK_Vehicles_DailyPrice",
                "[DailyPrice] >= 0");
        });

        // Configures the primary key.
        builder.HasKey(v => v.VehicleId);

        builder.Property(v => v.VehicleId)
            .ValueGeneratedOnAdd();

        // Vehicle manufacturer name.
        builder.Property(v => v.Brand)
            .IsRequired()
            .HasMaxLength(100);

        // Vehicle model name.
        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(100);

        // Manufacturing year is required.
        // Current-year validation is handled by the application layer.
        builder.Property(v => v.Year)
            .IsRequired();

        // License plate is required and must be unique.
        builder.Property(v => v.LicensePlate)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(v => v.LicensePlate)
            .IsUnique();

        // A vehicle must have at least one seat.
        builder.Property(v => v.Seats)
            .IsRequired();

        // Current catalogue price per rental day.
        builder.Property(v => v.DailyPrice)
            .IsRequired()
            .HasPrecision(10, 2);

        // Stores the vehicle photo path or URL.
        builder.Property(v => v.Photo)
            .HasMaxLength(500);

        // Optional vehicle description.
        builder.Property(v => v.Description)
            .HasMaxLength(1000);

        // Audit timestamps.
        builder.Property(v => v.CreatedAt)
            .HasColumnType("datetime2");

        builder.Property(v => v.UpdatedAt)
            .HasColumnType("datetime2");

        // Each vehicle belongs to exactly one category.
        // Historical vehicle data must not be removed through cascade deletion.
        builder.HasOne(v => v.Category)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        // Each vehicle has exactly one current operational status.
        // Deleting a status must not automatically delete vehicles.
        builder.HasOne(v => v.VehicleStatus)
            .WithMany(vs => vs.Vehicles)
            .HasForeignKey(v => v.VehicleStatusId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
