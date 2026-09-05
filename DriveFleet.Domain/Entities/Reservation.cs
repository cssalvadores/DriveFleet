using System.Collections.Generic;

namespace DriveFleet.Domain.Entities;

public class Reservation
{
    public int ReservationId { get; set; }

    public int UserId { get; set; }

    public int ReservationStatusId { get; set; }

    public decimal TotalValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;

    public ReservationStatus ReservationStatus { get; set; } = null!;

    public ICollection<ReservationVehicle> ReservationVehicles { get; set; }
        = new List<ReservationVehicle>();
}
