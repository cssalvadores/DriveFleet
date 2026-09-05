using System.Collections.Generic;

namespace DriveFleet.Domain.Entities;

public class Extra
{
    public int ExtraId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? Photo { get; set; }

    public bool Active { get; set; }

    public ICollection<ReservationVehicleExtra> ReservationVehicleExtras { get; set; }
        = new List<ReservationVehicleExtra>();
}
