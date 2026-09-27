namespace DriveFleet.Web.Models.Reservations;

/// <summary>
/// Represents the form used by a client to create a reservation.
/// </summary>
public class CreateReservationViewModel
{
    /// <summary>
    /// Gets or sets the vehicles available for selection.
    /// </summary>
    public List<CreateReservationVehicleViewModel> Vehicles { get; set; }
    = new();

    /// <summary>
    /// Gets or sets the identifier of a vehicle selected
    /// from the add-vehicle modal.
    /// </summary>
    public int? VehicleToAddId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of a vehicle selected
    /// to be removed from the reservation request.
    /// </summary>
    public int? VehicleToRemoveId { get; set; }

    /// <summary>
    /// Gets or sets the vehicles that can still be added
    /// to the reservation.
    /// </summary>
    public List<ReservationVehicleOptionViewModel> AvailableVehicles { get; set; }
        = new();
}
