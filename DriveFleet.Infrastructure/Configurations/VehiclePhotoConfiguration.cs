using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveFleet.Infrastructure.Data.Configurations;

/// <summary>
/// Configures the database mapping for vehicle catalog photos.
/// </summary>
public class VehiclePhotoConfiguration :
    IEntityTypeConfiguration<VehiclePhoto>
{
    /// <summary>
    /// Configures the VehiclePhoto entity.
    /// </summary>
    /// <param name="builder">
    /// Builder used to configure the entity mapping.
    /// </param>
    public void Configure(
        EntityTypeBuilder<VehiclePhoto> builder)
    {
        builder.ToTable(
            "VehiclePhotos",
            table =>
            {
                // Ensures that catalog photo positions remain between 1 and 3.
                table.HasCheckConstraint(
                    "CK_VehiclePhotos_DisplayOrder",
                    "[DisplayOrder] BETWEEN 1 AND 3");
            });

        builder.HasKey(
            photo => photo.VehiclePhotoId);

        builder.Property(
                photo => photo.FilePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(
                photo => photo.DisplayOrder)
            .IsRequired();

        builder.Property(
                photo => photo.CreatedAt)
            .IsRequired();

        // Prevents two photos from occupying the same
        // catalog position for the same vehicle.
        builder.HasIndex(
                photo => new
                {
                    photo.VehicleId,
                    photo.DisplayOrder
                })
            .IsUnique();

        builder.HasOne(
                photo => photo.Vehicle)
            .WithMany(
                vehicle => vehicle.VehiclePhotos)
            .HasForeignKey(
                photo => photo.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
