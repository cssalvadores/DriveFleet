using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the Category entity.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Maps the entity to the Categories table.
        builder.ToTable("Categories");

        // Configures the primary key.
        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.CategoryId)
            .ValueGeneratedOnAdd();

        // Category names are required and must be unique.
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.Name)
            .IsUnique();

        // Category description is optional.
        builder.Property(c => c.Description)
            .HasMaxLength(500);
    }
}
