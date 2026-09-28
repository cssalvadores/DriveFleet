using System.Globalization;
using System.Net;
using DriveFleet.Web.Models.Dashboard;
using DriveFleet.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Web.Controllers;

/// <summary>
/// Provides administrative dashboard statistics.
/// </summary>
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private const string AccessTokenName =
        "access_token";

    private const string CancelledReservationStatusName =
        "Cancelled";

    private const int MostRentedVehicleLimit = 5;

    private readonly ReservationApiClient
        _reservationApiClient;

    private readonly VehicleApiClient
        _vehicleApiClient;

    private readonly ILogger<DashboardController>
        _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DashboardController"/> class.
    /// </summary>
    public DashboardController(
        ReservationApiClient reservationApiClient,
        VehicleApiClient vehicleApiClient,
        ILogger<DashboardController> logger)
    {
        _reservationApiClient =
            reservationApiClient;

        _vehicleApiClient =
            vehicleApiClient;

        _logger =
            logger;
    }

    /// <summary>
    /// Displays the administrative dashboard
    /// for the selected reporting period.
    /// </summary>
    [HttpGet("/dashboard")]
    public async Task<IActionResult> Index(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var today =
            DateTime.Today;

        var selectedFromDate =
            fromDate?.Date ??
            new DateTime(
                today.Year,
                1,
                1);

        var selectedToDate =
            toDate?.Date ??
            today;

        if (selectedFromDate >
            selectedToDate)
        {
            return BadRequest(
                "The start date cannot be later than the end date.");
        }

        var accessToken =
            await HttpContext.GetTokenAsync(
                AccessTokenName);

        if (string.IsNullOrWhiteSpace(
            accessToken))
        {
            return await
                SignOutAndRedirectToLoginAsync();
        }

        try
        {
            var reservationsTask =
                _reservationApiClient.GetAllAsync(
                    accessToken,
                    statusId: null,
                    fromDate: null,
                    toDate: null,
                    cancellationToken:
                        cancellationToken);

            var vehiclesTask =
                _vehicleApiClient.GetAllAsync(
                    cancellationToken);

            await Task.WhenAll(
                reservationsTask,
                vehiclesTask);

            var reservationsResult =
                await reservationsTask;

            var vehiclesResult =
                await vehiclesTask;

            if (reservationsResult.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                return await
                    SignOutAndRedirectToLoginAsync();
            }

            if (reservationsResult.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (reservationsResult.StatusCode !=
                    HttpStatusCode.OK ||
                vehiclesResult.StatusCode !=
                    HttpStatusCode.OK)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    "The dashboard data could not be retrieved.");
            }

            var model =
                BuildDashboardViewModel(
                    reservationsResult.Reservations,
                    vehiclesResult.Vehicles,
                    selectedFromDate,
                    selectedToDate);

            return View(model);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while loading the administrative dashboard.");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "The dashboard service is temporarily unavailable.");
        }
    }

    /// <summary>
    /// Builds all dashboard statistics
    /// for the selected reporting period.
    /// </summary>
    private static DashboardViewModel
        BuildDashboardViewModel(
            IReadOnlyList<ReservationApiModel>
                reservations,
            IReadOnlyList<VehicleApiModel>
                vehicles,
            DateTime fromDate,
            DateTime toDate)
    {
        var reservationsInPeriod =
            reservations
                .Where(reservation =>
                    IsReservationInPeriod(
                        reservation,
                        fromDate,
                        toDate))
                .ToList();

        var revenueReservations =
            reservationsInPeriod
                .Where(reservation =>
                    !IsCancelled(
                        reservation))
                .ToList();

        return new DashboardViewModel
        {
            FromDate = fromDate,

            ToDate = toDate,

            TotalReservations =
                reservationsInPeriod.Count,

            TotalRevenue =
                revenueReservations.Sum(
                    reservation =>
                        reservation.TotalValue),

            TotalVehicles =
                vehicles.Count,

            FleetOccupancyRate =
                CalculateFleetOccupancyRate(
                    reservations,
                    vehicles.Count,
                    fromDate,
                    toDate),

            ReservationsByMonth =
                BuildReservationsByMonth(
                    reservationsInPeriod,
                    fromDate,
                    toDate),

            MostRentedVehicles =
                BuildMostRentedVehicles(
                    reservations,
                    fromDate,
                    toDate)
        };
    }

    /// <summary>
    /// Determines whether a reservation belongs
    /// to the selected reporting period.
    /// </summary>
    private static bool IsReservationInPeriod(
        ReservationApiModel reservation,
        DateTime fromDate,
        DateTime toDate)
    {
        if (reservation.Vehicles.Count == 0)
        {
            return false;
        }

        var reservationStartDate =
            reservation.Vehicles
                .Min(vehicle =>
                    vehicle.StartDate)
                .Date;

        return reservationStartDate >=
                   fromDate.Date &&
               reservationStartDate <=
                   toDate.Date;
    }

    /// <summary>
    /// Determines whether the reservation
    /// has been cancelled.
    /// </summary>
    private static bool IsCancelled(
        ReservationApiModel reservation)
    {
        return string.Equals(
            reservation.ReservationStatusName,
            CancelledReservationStatusName,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Builds monthly reservation statistics,
    /// including months with no reservations.
    /// </summary>
    private static List<MonthlyReservationViewModel>
        BuildReservationsByMonth(
            IReadOnlyCollection<ReservationApiModel>
                reservations,
            DateTime fromDate,
            DateTime toDate)
    {
        var countsByMonth =
            reservations
                .Where(reservation =>
                    reservation.Vehicles.Count > 0)
                .GroupBy(reservation =>
                {
                    var firstStartDate =
                        reservation.Vehicles
                            .Min(vehicle =>
                                vehicle.StartDate);

                    return new
                    {
                        firstStartDate.Year,
                        firstStartDate.Month
                    };
                })
                .ToDictionary(
                    group =>
                        (group.Key.Year,
                         group.Key.Month),
                    group =>
                        group.Count());

        var result =
            new List<MonthlyReservationViewModel>();

        var currentMonth =
            new DateTime(
                fromDate.Year,
                fromDate.Month,
                1);

        var lastMonth =
            new DateTime(
                toDate.Year,
                toDate.Month,
                1);

        while (currentMonth <=
               lastMonth)
        {
            countsByMonth.TryGetValue(
                (
                    currentMonth.Year,
                    currentMonth.Month
                ),
                out var reservationCount);

            result.Add(
                new MonthlyReservationViewModel
                {
                    Month =
                        currentMonth.ToString(
                            "MMM yyyy",
                            CultureInfo.InvariantCulture),

                    ReservationCount =
                        reservationCount
                });

            currentMonth =
                currentMonth.AddMonths(1);
        }

        return result;
    }

    /// <summary>
    /// Builds the ranking of the most rented vehicles
    /// during the selected period.
    /// </summary>
    private static List<MostRentedVehicleViewModel>
        BuildMostRentedVehicles(
            IReadOnlyCollection<ReservationApiModel>
                reservations,
            DateTime fromDate,
            DateTime toDate)
    {
        return reservations
            .Where(reservation =>
                !IsCancelled(
                    reservation))
            .SelectMany(reservation =>
                reservation.Vehicles)
            .Where(vehicle =>
                RentalOverlapsPeriod(
                    vehicle,
                    fromDate,
                    toDate))
            .GroupBy(vehicle =>
                vehicle.VehicleId)
            .Select(group =>
            {
                var vehicle =
                    group.First();

                return new MostRentedVehicleViewModel
                {
                    VehicleId =
                        vehicle.VehicleId,

                    VehicleName =
                        $"{vehicle.VehicleBrand} " +
                        $"{vehicle.VehicleModel}",

                    LicensePlate =
                        vehicle.LicensePlate,

                    RentalCount =
                        group.Count()
                };
            })
            .OrderByDescending(vehicle =>
                vehicle.RentalCount)
            .ThenBy(vehicle =>
                vehicle.VehicleName)
            .Take(
                MostRentedVehicleLimit)
            .ToList();
    }

    /// <summary>
    /// Calculates fleet occupancy based on
    /// vehicle rental time within the selected period.
    /// </summary>
    private static double CalculateFleetOccupancyRate(
        IReadOnlyCollection<ReservationApiModel>
            reservations,
        int totalVehicles,
        DateTime fromDate,
        DateTime toDate)
    {
        if (totalVehicles <= 0)
        {
            return 0;
        }

        var periodStart =
            fromDate.Date;

        var periodEndExclusive =
            toDate.Date.AddDays(1);

        var periodDays =
            (
                periodEndExclusive -
                periodStart
            ).TotalDays;

        if (periodDays <= 0)
        {
            return 0;
        }

        var occupiedDays =
            reservations
                .Where(reservation =>
                    !IsCancelled(
                        reservation))
                .SelectMany(reservation =>
                    reservation.Vehicles)
                .Sum(vehicle =>
                    CalculateOccupiedDays(
                        vehicle,
                        periodStart,
                        periodEndExclusive));

        var availableFleetDays =
            totalVehicles *
            periodDays;

        if (availableFleetDays <= 0)
        {
            return 0;
        }

        var occupancyRate =
            occupiedDays /
            availableFleetDays *
            100;

        return Math.Round(
            Math.Min(
                occupancyRate,
                100),
            2);
    }

    /// <summary>
    /// Calculates how many rental days
    /// fall inside the selected reporting period.
    /// </summary>
    private static double CalculateOccupiedDays(
        ReservationVehicleApiModel vehicle,
        DateTime periodStart,
        DateTime periodEndExclusive)
    {
        var rentalStart =
            vehicle.StartDate >
            periodStart
                ? vehicle.StartDate
                : periodStart;

        var rentalEnd =
            vehicle.EndDate <
            periodEndExclusive
                ? vehicle.EndDate
                : periodEndExclusive;

        if (rentalEnd <=
            rentalStart)
        {
            return 0;
        }

        return (
            rentalEnd -
            rentalStart
        ).TotalDays;
    }

    /// <summary>
    /// Determines whether a vehicle rental
    /// overlaps the selected reporting period.
    /// </summary>
    private static bool RentalOverlapsPeriod(
        ReservationVehicleApiModel vehicle,
        DateTime fromDate,
        DateTime toDate)
    {
        var periodStart =
            fromDate.Date;

        var periodEndExclusive =
            toDate.Date.AddDays(1);

        return vehicle.StartDate <
                   periodEndExclusive &&
               vehicle.EndDate >
                   periodStart;
    }

    /// <summary>
    /// Clears the local authentication cookie
    /// and redirects the user to the login page.
    /// </summary>
    private async Task<IActionResult>
        SignOutAndRedirectToLoginAsync()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme);

        return RedirectToAction(
            "Login",
            "Account");
    }
}
