using System.Net;

namespace DriveFleet.Mobile.Models.Reservations;

/// <summary>
/// Represents the result of a reservation
/// status action API request.
/// </summary>
public class ReservationActionApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public string? Detail { get; set; }
}
