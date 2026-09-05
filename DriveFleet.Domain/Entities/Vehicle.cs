using System.Collections.Generic;

namespace DriveFleet.Domain.Entities;

public class Vehicle
{
    public int VehicleId { get; set; }

    public string Brand { get; set; } = null!;

    public string Model { get; set; } = null!;

    public int Year { get; set; }

    public string LicensePlate { get; set; } = null!;

    public int Seats { get; set; }

    public decimal DailyPrice { get; set; }

    public string? Photo { get; set; }

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public int VehicleStatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Category Category { get; set; } = null!;

    public VehicleStatus VehicleStatus { get; set; } = null!;

    public ICollection<ReservationVehicle> ReservationVehicles { get; set; }
        = new List<ReservationVehicle>();
}
