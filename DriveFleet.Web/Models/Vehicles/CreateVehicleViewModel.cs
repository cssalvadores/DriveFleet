using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DriveFleet.Web.Models.Vehicles;

/// <summary>
/// Represents the form used to create a vehicle.
/// </summary>
public class CreateVehicleViewModel
{
    /// <summary>
    /// Gets or sets the vehicle brand.
    /// </summary>
    [Required]
    [StringLength(100)]
    [Display(Name = "Brand")]
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vehicle model.
    /// </summary>
    [Required]
    [StringLength(100)]
    [Display(Name = "Model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vehicle manufacturing year.
    /// </summary>
    [Range(1900, 2026)]
    [Display(Name = "Year")]
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the vehicle license plate.
    /// </summary>
    [Required]
    [StringLength(20)]
    [Display(Name = "License plate")]
    public string LicensePlate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of vehicle seats.
    /// </summary>
    [Range(1, 20)]
    [Display(Name = "Seats")]
    public int Seats { get; set; }

    /// <summary>
    /// Gets or sets the daily rental price.
    /// </summary>
    [Range(0.01, 999999.99)]
    [Display(Name = "Daily price")]
    public decimal DailyPrice { get; set; }

    /// <summary>
    /// Gets or sets the optional vehicle description.
    /// </summary>
    [StringLength(1000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the selected category identifier.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the selected vehicle status identifier.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Please select a vehicle status.")]
    [Display(Name = "Status")]
    public int VehicleStatusId { get; set; }

    /// <summary>
    /// Gets or sets the available vehicle categories.
    /// </summary>
    public List<SelectListItem> Categories { get; set; } = new();

    /// <summary>
    /// Gets or sets the available vehicle statuses.
    /// </summary>
    public List<SelectListItem> VehicleStatuses { get; set; } = new();
}
