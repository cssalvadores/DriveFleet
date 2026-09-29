using System.Net;

namespace DriveFleet.Mobile.Models.Vehicles;

/// <summary>
/// Represents the result of an authenticated
/// vehicle update request.
/// </summary>
public class UpdateVehicleApiResult
{
    public HttpStatusCode StatusCode { get; set; }

    public string? Detail { get; set; }
}
