using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Data;

/// <summary>
/// Main database context for DriveFleet.
/// Connects domain entities to Entity Framework Core.
/// </summary>
public class DriveFleetDbContext : DbContext
{
    public DriveFleetDbContext(
        DbContextOptions<DriveFleetDbContext> options)
        : base(options)
    {
    }

    // Access roles: Admin, Employee and Client.
    public DbSet<Role> Roles => Set<Role>();

    // Registered users.
    public DbSet<User> Users => Set<User>();

    // Vehicle categories.
    public DbSet<Category> Categories => Set<Category>();

    // Current operational vehicle statuses.
    public DbSet<VehicleStatus> VehicleStatuses => Set<VehicleStatus>();

    // Vehicle fleet.
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    // Reservation lifecycle statuses.
    public DbSet<ReservationStatus> ReservationStatuses
        => Set<ReservationStatus>();

    // Reservation headers.
    public DbSet<Reservation> Reservations => Set<Reservation>();

    // Vehicles assigned to reservations.
    // Each record has its own rental period.
    public DbSet<ReservationVehicle> ReservationVehicles
        => Set<ReservationVehicle>();

    // Catalogue of optional services.
    public DbSet<Extra> Extras => Set<Extra>();

    // Extras assigned to each reserved vehicle.
    public DbSet<ReservationVehicleExtra> ReservationVehicleExtras
        => Set<ReservationVehicleExtra>();

    // Vehicle reviews created after completed rentals.
    public DbSet<Review> Reviews => Set<Review>();

    // Tokens used for email confirmation.
    public DbSet<EmailConfirmationToken> EmailConfirmationTokens
        => Set<EmailConfirmationToken>();

    // Temporary tokens used for password recovery.
    public DbSet<PasswordResetToken> PasswordResetTokens
        => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically applies all IEntityTypeConfiguration<T>
        // implementations found in this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(DriveFleetDbContext).Assembly);
    }
}
