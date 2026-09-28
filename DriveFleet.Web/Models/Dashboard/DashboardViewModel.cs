namespace DriveFleet.Web.Models.Dashboard;

/// <summary>
/// Represents the administrative dashboard data.
/// </summary>
public class DashboardViewModel
{
    /// <summary>
    /// Gets or sets the first date included
    /// in the dashboard reporting period.
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// Gets or sets the last date included
    /// in the dashboard reporting period.
    /// </summary>
    public DateTime ToDate { get; set; }

    /// <summary>
    /// Gets or sets the number of reservations
    /// included in the selected period.
    /// </summary>
    public int TotalReservations { get; set; }

    /// <summary>
    /// Gets or sets the total reservation value
    /// for the selected reporting period.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Gets or sets the total number
    /// of vehicles in the fleet.
    /// </summary>
    public int TotalVehicles { get; set; }

    /// <summary>
    /// Gets or sets the fleet occupancy rate
    /// for the selected period.
    /// </summary>
    public double FleetOccupancyRate { get; set; }

    /// <summary>
    /// Gets or sets reservation totals grouped by month.
    /// </summary>
    public List<MonthlyReservationViewModel>
        ReservationsByMonth
    { get; set; } = new();

    /// <summary>
    /// Gets or sets the most rented vehicles.
    /// </summary>
    public List<MostRentedVehicleViewModel>
        MostRentedVehicles
    { get; set; } = new();
}

/// <summary>
/// Represents the reservation count for one calendar month.
/// </summary>
public class MonthlyReservationViewModel
{
    /// <summary>
    /// Gets or sets the month label displayed on the chart.
    /// </summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of reservations
    /// associated with the month.
    /// </summary>
    public int ReservationCount { get; set; }
}

/// <summary>
/// Represents vehicle rental frequency
/// in the administrative dashboard.
/// </summary>
public class MostRentedVehicleViewModel
{
    /// <summary>
    /// Gets or sets the vehicle identifier.
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle display name.
    /// </summary>
    public string VehicleName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vehicle license plate.
    /// </summary>
    public string LicensePlate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of rentals
    /// associated with the vehicle.
    /// </summary>
    public int RentalCount { get; set; }
}