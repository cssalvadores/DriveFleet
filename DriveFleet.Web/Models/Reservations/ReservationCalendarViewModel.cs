namespace DriveFleet.Web.Models.Reservations;

/// <summary>
/// Represents the reservation calendar displayed
/// to an administrator.
/// </summary>
public class ReservationCalendarViewModel
{
    public string ViewMode { get; set; } = "month";

    public DateTime ReferenceDate { get; set; }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public IReadOnlyList<ReservationCalendarEntryViewModel> Entries { get; set; }
        = Array.Empty<ReservationCalendarEntryViewModel>();

    public string? ErrorMessage { get; set; }

    public bool IsWeekView =>
        string.Equals(
            ViewMode,
            "week",
            StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Represents one reserved vehicle displayed
/// in the reservation calendar.
/// </summary>
public class ReservationCalendarEntryViewModel
{
    public int ReservationId { get; set; }

    public string UserFullName { get; set; } = string.Empty;

    public string ReservationStatusName { get; set; } = string.Empty;

    public string VehicleBrand { get; set; } = string.Empty;

    public string VehicleModel { get; set; } = string.Empty;

    public string LicensePlate { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}
