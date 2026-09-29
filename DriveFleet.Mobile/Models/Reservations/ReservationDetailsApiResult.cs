using System.Net;

namespace DriveFleet.Mobile.Models.Reservations;

/// <summary>
/// Represents the result of a reservation
/// details API request.
/// </summary>
public class ReservationDetailsApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public ReservationModel? Reservation { get; set; }

    public string? Detail { get; set; }
}
