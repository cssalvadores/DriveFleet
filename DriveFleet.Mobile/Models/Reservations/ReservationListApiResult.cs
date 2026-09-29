using System.Net;

namespace DriveFleet.Mobile.Models.Reservations;

/// <summary>
/// Represents the result of a reservation
/// list request.
/// </summary>
public class ReservationListApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public List<ReservationModel> Reservations { get; set; } =
        new();

    public string? Detail { get; set; }
}
