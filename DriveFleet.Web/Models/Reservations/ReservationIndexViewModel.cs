using System.ComponentModel.DataAnnotations;
using DriveFleet.Domain.Constants;

namespace DriveFleet.Web.Models.Reservations;

/// <summary>
/// Represents the reservation list page,
/// including filtering criteria and results.
/// </summary>
public class ReservationIndexViewModel
{
    /// <summary>
    /// Gets or sets the selected reservation status identifier.
    /// </summary>
    [Display(Name = "Status")]
    public int? StatusId { get; set; }

    /// <summary>
    /// Gets or sets the first date of the rental period filter.
    /// </summary>
    [DataType(DataType.Date)]
    [Display(Name = "From date")]
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Gets or sets the last date of the rental period filter.
    /// </summary>
    [DataType(DataType.Date)]
    [Display(Name = "To date")]
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Gets or sets the reservations returned by the API.
    /// </summary>
    public IReadOnlyList<ReservationViewModel> Reservations { get; set; }
        = Array.Empty<ReservationViewModel>();

    /// <summary>
    /// Gets or sets an error message returned while
    /// applying reservation filters.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the reservation statuses available
    /// in the filter.
    /// </summary>
    public IReadOnlyList<ReservationStatusOptionViewModel> StatusOptions { get; }
        =
        [
            new()
            {
                Id = ReservationStatusIds.Pending,
                Name = "Pending"
            },
            new()
            {
                Id = ReservationStatusIds.Active,
                Name = "Active"
            },
            new()
            {
                Id = ReservationStatusIds.Completed,
                Name = "Completed"
            },
            new()
            {
                Id = ReservationStatusIds.Cancelled,
                Name = "Cancelled"
            }
        ];

    /// <summary>
    /// Gets a value indicating whether at least
    /// one reservation filter is currently active.
    /// </summary>
    public bool HasActiveFilters =>
        StatusId.HasValue ||
        FromDate.HasValue ||
        ToDate.HasValue;
}

/// <summary>
/// Represents a reservation status displayed
/// in the reservation filter.
/// </summary>
public class ReservationStatusOptionViewModel
{
    /// <summary>
    /// Gets or initializes the reservation status identifier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets or initializes the reservation status name.
    /// </summary>
    public string Name { get; init; } = string.Empty;
}
