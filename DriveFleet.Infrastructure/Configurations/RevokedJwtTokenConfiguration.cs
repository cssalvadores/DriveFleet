using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveFleet.Infrastructure.Configurations;

/// <summary>
/// Configures the database mapping for the RevokedJwtToken entity.
/// </summary>
public class RevokedJwtTokenConfiguration
    : IEntityTypeConfiguration<RevokedJwtToken>
{
    public void Configure(
        EntityTypeBuilder<RevokedJwtToken> builder)
    {
        // Maps the entity to the RevokedJwtTokens table.
        builder.ToTable("RevokedJwtTokens");

        // Configures the primary key.
        builder.HasKey(t => t.RevokedJwtTokenId);

        builder.Property(t => t.RevokedJwtTokenId)
            .ValueGeneratedOnAdd();

        // Stores the unique identifier contained in the JWT jti claim.
        builder.Property(t => t.Jti)
            .IsRequired()
            .HasMaxLength(64);

        // Prevents the same JWT from being registered as revoked more than once.
        builder.HasIndex(t => t.Jti)
            .IsUnique();

        // Defines when the revoked JWT naturally expires.
        builder.Property(t => t.ExpiresAt)
            .IsRequired()
            .HasColumnType("datetime2");

        // Records when the JWT was revoked.
        builder.Property(t => t.RevokedAt)
            .IsRequired()
            .HasColumnType("datetime2");
    }
}
