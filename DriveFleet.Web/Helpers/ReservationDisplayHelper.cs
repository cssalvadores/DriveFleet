namespace DriveFleet.Web.Helpers;

/// <summary>
/// Provides presentation helpers used by reservation views.
/// </summary>
public static class ReservationDisplayHelper
{
    /// <summary>
    /// Returns the Bootstrap badge class associated
    /// with a reservation status.
    /// </summary>
    /// <param name="statusName">
    /// The reservation status name.
    /// </param>
    /// <returns>
    /// The CSS class used to render the status badge.
    /// </returns>
    public static string GetStatusBadgeClass(
        string statusName)
    {
        return statusName switch
        {
            "Pending" => "text-bg-warning",
            "Active" => "text-bg-primary",
            "Completed" => "text-bg-success",
            "Cancelled" => "text-bg-secondary",
            _ => "text-bg-light"
        };
    }

    /// <summary>
    /// Formats a date and time for display.
    /// </summary>
    /// <param name="dateTime">
    /// The date and time to format.
    /// </param>
    /// <returns>
    /// The formatted local date and time.
    /// </returns>
    public static string FormatDateTime(
        DateTime dateTime)
    {
        return dateTime
            .ToLocalTime()
            .ToString("dd/MM/yyyy HH:mm");
    }

    /// <summary>
    /// Returns a readable label for the number
    /// of vehicles included in a reservation.
    /// </summary>
    /// <param name="vehicleCount">
    /// The number of vehicles.
    /// </param>
    /// <returns>
    /// A singular or plural vehicle count label.
    /// </returns>
    public static string GetVehicleCountText(
        int vehicleCount)
    {
        return vehicleCount == 1
            ? "1 vehicle"
            : $"{vehicleCount} vehicles";
    }
}
