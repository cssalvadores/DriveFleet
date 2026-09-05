using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the Role entity.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Maps the entity to the Roles table.
        builder.ToTable("Roles");

        // Configures the primary key.
        builder.HasKey(r => r.RoleId);

        builder.Property(r => r.RoleId)
            .ValueGeneratedOnAdd();

        // Role names are required and limited to 50 characters.
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        // Prevents duplicate role names.
        builder.HasIndex(r => r.Name)
            .IsUnique();
    }
}
