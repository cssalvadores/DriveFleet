namespace DriveFleet.Application.DTOs.Reservations;

public class CreateReservationRequest
{
    public ICollection<CreateReservationVehicleRequest> Vehicles { get; set; }
        = new List<CreateReservationVehicleRequest>();
}
