using DriveFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveFleet.Infrastructure.Data.Seed;

/// <summary>
/// Defines the initial data required by the DriveFleet database.
/// </summary>
public static class DatabaseSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedRoles(modelBuilder);
        SeedVehicleStatuses(modelBuilder);
        SeedReservationStatuses(modelBuilder);
        SeedExtras(modelBuilder);
        SeedCategories(modelBuilder);
    }

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        // Structural roles required by the application.
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                RoleId = 1,
                Name = "Admin"
            },
            new Role
            {
                RoleId = 2,
                Name = "Employee"
            },
            new Role
            {
                RoleId = 3,
                Name = "Client"
            }
        );
    }

    private static void SeedVehicleStatuses(ModelBuilder modelBuilder)
    {
        // Structural vehicle statuses used by fleet management.
        modelBuilder.Entity<VehicleStatus>().HasData(
            new VehicleStatus
            {
                VehicleStatusId = 1,
                Name = "Available"
            },
            new VehicleStatus
            {
                VehicleStatusId = 2,
                Name = "Reserved"
            },
            new VehicleStatus
            {
                VehicleStatusId = 3,
                Name = "Unavailable"
            }
        );
    }

    private static void SeedReservationStatuses(ModelBuilder modelBuilder)
    {
        // Structural statuses used throughout the reservation lifecycle.
        modelBuilder.Entity<ReservationStatus>().HasData(
            new ReservationStatus
            {
                ReservationStatusId = 1,
                Name = "Pending"
            },
            new ReservationStatus
            {
                ReservationStatusId = 2,
                Name = "Active"
            },
            new ReservationStatus
            {
                ReservationStatusId = 3,
                Name = "Completed"
            },
            new ReservationStatus
            {
                ReservationStatusId = 4,
                Name = "Cancelled"
            }
        );
    }

    private static void SeedExtras(ModelBuilder modelBuilder)
    {
        // Initial catalogue data.
        // Extra identifiers must never be treated as business constants.
        modelBuilder.Entity<Extra>().HasData(
            new Extra
            {
                ExtraId = 1,
                Name = "GPS",
                Description = "Portable GPS navigation system.",
                Price = 8.00m,
                Active = true
            },
            new Extra
            {
                ExtraId = 2,
                Name = "Baby Seat",
                Description = "Child safety seat for the rental vehicle.",
                Price = 6.00m,
                Active = true
            },
            new Extra
            {
                ExtraId = 3,
                Name = "Portable Wi-Fi",
                Description = "Portable Wi-Fi hotspot for internet access during the rental.",
                Price = 10.00m,
                Active = true
            }
        );
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        // Initial vehicle categories used by the rental catalogue.
        // Category identifiers must not be treated as business constants.
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                CategoryId = 1,
                Name = "Economy",
                Description = "Affordable vehicles designed for efficient everyday travel."
            },
            new Category
            {
                CategoryId = 2,
                Name = "Compact",
                Description = "Compact vehicles suitable for city driving and short trips."
            },
            new Category
            {
                CategoryId = 3,
                Name = "SUV",
                Description = "Spacious vehicles suitable for families and longer journeys."
            },
            new Category
            {
                CategoryId = 4,
                Name = "Luxury",
                Description = "Premium vehicles offering additional comfort and equipment."
            },
            new Category
            {
                CategoryId = 5,
                Name = "Van",
                Description = "Large vehicles designed for additional passengers or luggage."
            }
        );
    }
}
