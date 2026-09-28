using System.Net;

namespace DriveFleet.Mobile.Models.Vehicles;

/// <summary>
/// Represents the result of a vehicle list
/// request made by the mobile application.
/// </summary>
public class VehicleListApiResult
{
    /// <summary>
    /// Gets or sets the HTTP status code
    /// returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicles returned
    /// by the API.
    /// </summary>
    public List<VehicleModel> Vehicles { get; set; } =
        new();

    /// <summary>
    /// Gets or sets the error detail returned
    /// by the API when available.
    /// </summary>
    public string? Detail { get; set; }
}