using System.Collections.Generic;

namespace DriveFleet.Domain.Entities;

public class Review
{
    public int ReviewId { get; set; }

    public int ReservationVehicleId { get; set; }

    public byte Stars { get; set; }

    public string? Comment { get; set; }

    public bool IsVisible { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ReservationVehicle ReservationVehicle { get; set; } = null!;
}
