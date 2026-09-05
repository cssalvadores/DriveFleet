using System.Collections.Generic;

namespace DriveFleet.Domain.Entities;

public class ReservationVehicleExtra
{
    public int ReservationVehicleExtraId { get; set; }

    public int ReservationVehicleId { get; set; }

    public int ExtraId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public ReservationVehicle ReservationVehicle { get; set; } = null!;

    public Extra Extra { get; set; } = null!;
}
