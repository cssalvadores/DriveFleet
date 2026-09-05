using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the Extra entity.
/// </summary>
public class ExtraConfiguration : IEntityTypeConfiguration<Extra>
{
    public void Configure(EntityTypeBuilder<Extra> builder)
    {
        // Maps the entity to the Extras table and defines
        // database-level integrity constraints.
        builder.ToTable("Extras", table =>
        {
            table.HasCheckConstraint(
                "CK_Extras_Price",
                "[Price] >= 0");
        });

        // Configures the primary key.
        builder.HasKey(e => e.ExtraId);

        builder.Property(e => e.ExtraId)
            .ValueGeneratedOnAdd();

        // Extra names are required and must be unique.
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => e.Name)
            .IsUnique();

        // Optional description of the extra.
        builder.Property(e => e.Description)
            .HasMaxLength(500);

        // Current catalogue price.
        builder.Property(e => e.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        // Stores the extra photo path or URL.
        builder.Property(e => e.Photo)
            .HasMaxLength(500);

        // Extras are active by default.
        builder.Property(e => e.Active)
            .HasDefaultValue(true);
    }
}
