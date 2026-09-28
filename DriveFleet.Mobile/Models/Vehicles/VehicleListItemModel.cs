namespace DriveFleet.Mobile.Models.Vehicles;

/// <summary>
/// Represents a vehicle prepared for display
/// in the mobile fleet list.
/// </summary>
public class VehicleListItemModel
{
    public int VehicleId { get; set; }

    public string Brand { get; set; } =
        string.Empty;

    public string Model { get; set; } =
        string.Empty;

    public int Year { get; set; }

    public string LicensePlate { get; set; } =
        string.Empty;

    public int Seats { get; set; }

    public decimal DailyPrice { get; set; }

    public string CategoryName { get; set; } =
        string.Empty;

    public int VehicleStatusId { get; set; }

    public string VehicleStatusName { get; set; } =
        string.Empty;

    public string? MainPhotoUrl { get; set; }

    /// <summary>
    /// Gets whether the vehicle has a catalog
    /// photo that can be displayed.
    /// </summary>
    public bool HasPhoto =>
        !string.IsNullOrWhiteSpace(
            MainPhotoUrl);

    /// <summary>
    /// Gets the complete vehicle display name.
    /// </summary>
    public string DisplayName =>
        $"{Brand} {Model}";
}
