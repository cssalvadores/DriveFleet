namespace DriveFleet.Application.DTOs.Reservations;

public class ReservationResponse
{
    public int ReservationId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who owns the reservation.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the full name of the user who owns the reservation.
    /// </summary>
    public string UserFullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the user who owns the reservation.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    public int ReservationStatusId { get; set; }

    public string ReservationStatusName { get; set; } = string.Empty;

    public decimal TotalValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<ReservationVehicleResponse> Vehicles { get; set; }
        = new List<ReservationVehicleResponse>();
}
