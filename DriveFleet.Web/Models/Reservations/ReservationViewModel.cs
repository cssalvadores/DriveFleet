using DriveFleet.Domain.Constants;

namespace DriveFleet.Web.Models.Reservations;

/// <summary>
/// Represents reservation information displayed by the Web application.
/// </summary>
public class ReservationViewModel
{
    /// <summary>
    /// Gets or sets the reservation identifier.
    /// </summary>
    public int ReservationId { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the reservation owner's full name.
    /// </summary>
    public string UserFullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reservation owner's email address.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reservation status identifier.
    /// </summary>
    public int ReservationStatusId { get; set; }

    /// <summary>
    /// Gets or sets the reservation status name.
    /// </summary>
    public string ReservationStatusName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total reservation value.
    /// </summary>
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Gets or sets the reservation creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last reservation update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the vehicles included in the reservation.
    /// </summary>
    public ICollection<ReservationVehicleViewModel> Vehicles { get; set; }
        = new List<ReservationVehicleViewModel>();

    /// <summary>
    /// Gets a value indicating whether the reservation
    /// can still be cancelled.
    /// </summary>
    public bool CanCancel =>
        ReservationStatusId ==
        ReservationStatusIds.Pending;

    /// <summary>
    /// Gets a value indicating whether the reservation
    /// can be started by authorized staff.
    /// </summary>
    public bool CanStart =>
        ReservationStatusId ==
        ReservationStatusIds.Pending;

    /// <summary>
    /// Gets a value indicating whether the reservation
    /// can be completed by authorized staff.
    /// </summary>
    public bool CanComplete =>
        ReservationStatusId ==
        ReservationStatusIds.Active;

    /// <summary>
    /// Gets a value indicating whether the authenticated client
    /// may still cancel the reservation.
    /// </summary>
    public bool CanClientCancel =>
        CanCancel &&
        Vehicles.Count > 0 &&
        Vehicles.Min(vehicle => vehicle.StartDate) >
            DateTime.UtcNow;
}
