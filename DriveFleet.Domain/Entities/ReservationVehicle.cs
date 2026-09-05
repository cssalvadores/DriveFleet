using System.Collections.Generic;

namespace DriveFleet.Domain.Entities;

public class ReservationVehicle
{
    public int ReservationVehicleId { get; set; }

    public int ReservationId { get; set; }

    public int VehicleId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal DailyPrice { get; set; }

    public Reservation Reservation { get; set; } = null!;

    public Vehicle Vehicle { get; set; } = null!;

    public ICollection<ReservationVehicleExtra> ReservationVehicleExtras { get; set; }
        = new List<ReservationVehicleExtra>();

    public Review? Review { get; set; }
}
