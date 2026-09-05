using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the VehicleStatus entity.
/// </summary>
public class VehicleStatusConfiguration
    : IEntityTypeConfiguration<VehicleStatus>
{
    public void Configure(EntityTypeBuilder<VehicleStatus> builder)
    {
        // Maps the entity to the VehicleStatus table.
        builder.ToTable("VehicleStatus");

        // Configures the primary key.
        builder.HasKey(vs => vs.VehicleStatusId);

        builder.Property(vs => vs.VehicleStatusId)
            .ValueGeneratedOnAdd();

        // Vehicle status names are required and must be unique.
        builder.Property(vs => vs.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(vs => vs.Name)
            .IsUnique();
    }
}
