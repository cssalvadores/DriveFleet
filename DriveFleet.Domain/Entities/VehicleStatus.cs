using System.Collections.Generic;

namespace DriveFleet.Domain.Entities;

public class VehicleStatus
{
    public int VehicleStatusId { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
