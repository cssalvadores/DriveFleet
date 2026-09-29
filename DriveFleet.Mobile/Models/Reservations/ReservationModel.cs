namespace DriveFleet.Mobile.Models.Reservations;

/// <summary>
/// Represents a reservation returned
/// by the DriveFleet API.
/// </summary>
public class ReservationModel
{
    public int ReservationId { get; set; }

    public int UserId { get; set; }

    public string UserFullName { get; set; } =
        string.Empty;

    public string UserEmail { get; set; } =
        string.Empty;

    public int ReservationStatusId { get; set; }

    public string ReservationStatusName { get; set; } =
        string.Empty;

    public decimal TotalValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<ReservationVehicleModel> Vehicles { get; set; } =
        new();
}
