using System.Net;

namespace DriveFleet.Mobile.Models.Vehicles;

/// <summary>
/// Represents the result of a single vehicle
/// request made by the mobile application.
/// </summary>
public class VehicleApiResult
{
    /// <summary>
    /// Gets or sets the HTTP status code
    /// returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle returned
    /// by the API.
    /// </summary>
    public VehicleModel? Vehicle { get; set; }

    /// <summary>
    /// Gets or sets the error detail returned
    /// by the API when available.
    /// </summary>
    public string? Detail { get; set; }
}
