using System.ComponentModel.DataAnnotations;

namespace DriveFleet.Application.DTOs.Vehicles;

/// <summary>
/// Represents the information required to update a vehicle.
/// </summary>
public class UpdateVehicleRequest
{
    [Required]
    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [Range(1900, 2026)]
    public int Year { get; set; }

    [Required]
    [MaxLength(20)]
    public string LicensePlate { get; set; } = string.Empty;

    [Range(1, 20)]
    public int Seats { get; set; }

    [Range(0.01, 999999.99)]
    public decimal DailyPrice { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int VehicleStatusId { get; set; }
}
