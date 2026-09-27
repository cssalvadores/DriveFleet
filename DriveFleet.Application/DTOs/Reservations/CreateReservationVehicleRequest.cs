namespace DriveFleet.Application.DTOs.Reservations;

public class CreateReservationVehicleRequest
{
    public int VehicleId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public ICollection<CreateReservationExtraRequest> Extras { get; set; }
        = new List<CreateReservationExtraRequest>();
}
