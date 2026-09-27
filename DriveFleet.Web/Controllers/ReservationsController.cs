using System.Net;
using DriveFleet.Web.Models.Reservations;
using DriveFleet.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Web.Controllers;

/// <summary>
/// Provides reservation-related pages for authenticated users.
/// </summary>
[Authorize]
public class ReservationsController : Controller
{
    private const string ClientRole = "Client";
    private const string AdminRole = "Admin";
    private const string EmployeeRole = "Employee";

    private const string StaffRoles =
        AdminRole + "," + EmployeeRole;

    private const string AccessTokenName = "access_token";
    private const string ReservationErrorViewName = "ReservationError";
    private const string CreateViewName = "Create";

    private const string ReservationSuccessKey = "ReservationSuccess";
    private const string ReservationErrorKey = "ReservationError";

    private const string UnavailableVehicleStatusName = "Unavailable";

    private const string CalendarMonthView = "month";
    private const string CalendarWeekView = "week";

    private const string CancelledReservationStatusName =
        "Cancelled";

    private const int MainPhotoDisplayOrder = 1;
    private const int DefaultRentalStartHour = 10;
    private const int DefaultRentalDurationDays = 1;

    private readonly ReservationApiClient _reservationApiClient;
    private readonly VehicleApiClient _vehicleApiClient;
    private readonly ExtraApiClient _extraApiClient;
    private readonly ILogger<ReservationsController> _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReservationsController"/> class.
    /// </summary>
    /// <param name="reservationApiClient">
    /// Client used to communicate with reservation endpoints
    /// exposed by the DriveFleet API.
    /// </param>
    /// <param name="vehicleApiClient">
    /// Client used to retrieve vehicle catalog information.
    /// </param>
    /// <param name="extraApiClient">
    /// Client used to retrieve active reservation extras.
    /// </param>
    /// <param name="logger">
    /// Logger used to record unexpected communication errors.
    /// </param>
    public ReservationsController(
        ReservationApiClient reservationApiClient,
        VehicleApiClient vehicleApiClient,
        ExtraApiClient extraApiClient,
        ILogger<ReservationsController> logger)
    {
        _reservationApiClient = reservationApiClient;
        _vehicleApiClient = vehicleApiClient;
        _extraApiClient = extraApiClient;
        _logger = logger;
    }

    /// <summary>
    /// Displays reservations available to the authenticated user
    /// using the supplied filtering criteria.
    /// </summary>
    /// <param name="statusId">
    /// The optional reservation status identifier.
    /// </param>
    /// <param name="fromDate">
    /// The optional first rental date.
    /// </param>
    /// <param name="toDate">
    /// The optional last rental date.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The reservation list page.
    /// </returns>
    [HttpGet("/reservations")]
    public async Task<IActionResult> Index(
        [FromQuery] int? statusId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var viewModel =
            new ReservationIndexViewModel
            {
                StatusId = statusId,
                FromDate = fromDate,
                ToDate = toDate
            };

        var accessToken =
            await GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return await SignOutAndRedirectToLoginAsync();
        }

        try
        {
            var result =
                await GetReservationListAsync(
                    accessToken,
                    viewModel,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await SignOutAndRedirectToLoginAsync();
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                viewModel.ErrorMessage =
                    string.IsNullOrWhiteSpace(result.Detail)
                        ? "The selected reservation filters are invalid."
                        : result.Detail;

                return View(viewModel);
            }

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return View(
                    ReservationErrorViewName);
            }

            viewModel.Reservations =
                result.Reservations
                    .Select(MapReservation)
                    .ToList();

            return View(viewModel);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while retrieving reservations.");

            return View(
                ReservationErrorViewName);
        }
    }

    /// <summary>
    /// Displays the reservation calendar for administrators.
    /// </summary>
    /// <param name="view">
    /// The requested calendar view: month or week.
    /// </param>
    /// <param name="date">
    /// The date used as the calendar reference point.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The reservation calendar page.
    /// </returns>
    [Authorize(Roles = AdminRole)]
    [HttpGet("/reservations/calendar")]
    public async Task<IActionResult> Calendar(
        [FromQuery] string? view,
        [FromQuery] DateTime? date,
        CancellationToken cancellationToken)
    {
        var viewMode =
            string.Equals(
                view,
                CalendarWeekView,
                StringComparison.OrdinalIgnoreCase)
                ? CalendarWeekView
                : CalendarMonthView;

        var referenceDate =
            (date ?? DateTime.Today).Date;

        var (periodStart, periodEnd) =
            GetCalendarPeriod(
                referenceDate,
                viewMode);

        var viewModel =
            new ReservationCalendarViewModel
            {
                ViewMode = viewMode,
                ReferenceDate = referenceDate,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd
            };

        var accessToken =
            await GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return await SignOutAndRedirectToLoginAsync();
        }

        try
        {
            var result =
                await _reservationApiClient.GetAllAsync(
                    accessToken,
                    statusId: null,
                    fromDate: periodStart,
                    toDate: periodEnd,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await SignOutAndRedirectToLoginAsync();
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                viewModel.ErrorMessage =
                    string.IsNullOrWhiteSpace(result.Detail)
                        ? "The selected calendar period is invalid."
                        : result.Detail;

                return View(viewModel);
            }

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return View(
                    ReservationErrorViewName);
            }

            viewModel.Entries =
                MapCalendarEntries(
                    result.Reservations);

            return View(viewModel);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while retrieving the reservation calendar.");

            return View(
                ReservationErrorViewName);
        }
    }

    /// <summary>
    /// Retrieves the reservation list available to the
    /// authenticated user using the selected filters.
    /// </summary>
    /// <param name="accessToken">
    /// The authenticated user's JWT access token.
    /// </param>
    /// <param name="viewModel">
    /// The reservation list filtering criteria.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The reservation list result returned by the API.
    /// </returns>
    private Task<ReservationListApiResult> GetReservationListAsync(
        string accessToken,
        ReservationIndexViewModel viewModel,
        CancellationToken cancellationToken)
    {
        return CanManageAllReservations()
            ? _reservationApiClient.GetAllAsync(
                accessToken,
                viewModel.StatusId,
                viewModel.FromDate,
                viewModel.ToDate,
                cancellationToken)
            : _reservationApiClient.GetMineAsync(
                accessToken,
                viewModel.StatusId,
                viewModel.FromDate,
                viewModel.ToDate,
                cancellationToken);
    }

    /// <summary>
    /// Displays the details of a reservation.
    /// </summary>
    /// <param name="reservationId">
    /// The identifier of the reservation to display.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The reservation details page.
    /// </returns>
    [HttpGet("/reservations/{reservationId:int}")]
    public async Task<IActionResult> Details(
        int reservationId,
        CancellationToken cancellationToken)
    {
        var accessToken =
            await GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return await SignOutAndRedirectToLoginAsync();
        }

        try
        {
            var result =
                await _reservationApiClient.GetByIdAsync(
                    accessToken,
                    reservationId,
                    cancellationToken);

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await SignOutAndRedirectToLoginAsync();
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (result.StatusCode == HttpStatusCode.NotFound ||
                result.Reservation is null)
            {
                return NotFound();
            }

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return View(
                    ReservationErrorViewName);
            }

            return View(
                MapReservation(
                    result.Reservation));
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while retrieving reservation {ReservationId}.",
                reservationId);

            return View(
                ReservationErrorViewName);
        }
    }

    /// <summary>
    /// Displays the reservation creation form starting with
    /// the vehicle selected from the catalog.
    /// </summary>
    /// <param name="vehicleId">
    /// The identifier of the vehicle selected by the client.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// The reservation creation page for the selected vehicle.
    /// </returns>
    [Authorize(Roles = ClientRole)]
    [HttpGet("/reservations/create")]
    public async Task<IActionResult> Create(
        int? vehicleId,
        CancellationToken cancellationToken)
    {
        if (!vehicleId.HasValue ||
            vehicleId.Value <= 0)
        {
            return RedirectToAction(
                "Index",
                "Vehicles");
        }

        try
        {
            var viewModel =
                new CreateReservationViewModel
                {
                    Vehicles =
                    [
                        new CreateReservationVehicleViewModel
                        {
                            VehicleId = vehicleId.Value
                        }
                    ]
                };

            return await ReloadCreateViewAsync(
                viewModel,
                cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while preparing the reservation creation form.");

            return View(
                ReservationErrorViewName);
        }
    }

    /// <summary>
    /// Processes the reservation creation form for an authenticated client.
    /// </summary>
    /// <param name="viewModel">
    /// The vehicles, rental periods and extras selected by the client.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if the client disconnects.
    /// </param>
    /// <returns>
    /// A redirect to the reservation details page when creation succeeds,
    /// or the creation form when further interaction is required.
    /// </returns>
    [Authorize(Roles = ClientRole)]
    [HttpPost("/reservations/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateReservationViewModel viewModel,
        CancellationToken cancellationToken)
    {
        try
        {
            if (viewModel.VehicleToRemoveId.HasValue)
            {
                return await HandleRemoveVehicleAsync(
                    viewModel,
                    cancellationToken);
            }

            if (viewModel.VehicleToAddId.HasValue)
            {
                return await HandleAddVehicleAsync(
                    viewModel,
                    cancellationToken);
            }

            if (!ValidateReservationDraft(
                viewModel))
            {
                return await ReloadCreateViewAsync(
                    viewModel,
                    cancellationToken);
            }

            return await SubmitReservationAsync(
                viewModel,
                cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while creating a reservation.");

            return View(
                ReservationErrorViewName);
        }
    }

    /// <summary>
    /// Cancels a pending reservation.
    /// </summary>
    [HttpPost("/reservations/{reservationId:int}/cancel")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Cancel(
        int reservationId,
        CancellationToken cancellationToken)
    {
        return HandleReservationActionAsync(
            reservationId,
            _reservationApiClient.CancelAsync,
            "The reservation was cancelled successfully.",
            "The reservation cannot be cancelled.",
            "cancel",
            cancellationToken);
    }

    /// <summary>
    /// Starts a pending reservation for authorized staff.
    /// </summary>
    [Authorize(Roles = StaffRoles)]
    [HttpPost("/reservations/{reservationId:int}/start")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Start(
        int reservationId,
        CancellationToken cancellationToken)
    {
        return HandleReservationActionAsync(
            reservationId,
            _reservationApiClient.StartAsync,
            "The reservation was started successfully.",
            "The reservation cannot be started.",
            "start",
            cancellationToken);
    }

    /// <summary>
    /// Completes an active reservation for authorized staff.
    /// </summary>
    [Authorize(Roles = StaffRoles)]
    [HttpPost("/reservations/{reservationId:int}/complete")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Complete(
        int reservationId,
        CancellationToken cancellationToken)
    {
        return HandleReservationActionAsync(
            reservationId,
            _reservationApiClient.CompleteAsync,
            "The reservation was completed successfully.",
            "The reservation cannot be completed.",
            "complete",
            cancellationToken);
    }

    /// <summary>
    /// Executes a reservation status action
    /// and handles the common HTTP outcomes.
    /// </summary>
    private async Task<IActionResult> HandleReservationActionAsync(
        int reservationId,
        Func<
            string,
            int,
            CancellationToken,
            Task<ReservationActionApiResult>> action,
        string successMessage,
        string conflictFallbackMessage,
        string operationName,
        CancellationToken cancellationToken)
    {
        var accessToken =
            await GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return await SignOutAndRedirectToLoginAsync();
        }

        try
        {
            var result =
                await action(
                    accessToken,
                    reservationId,
                    cancellationToken);

            if (result.StatusCode ==
                HttpStatusCode.NoContent)
            {
                TempData[ReservationSuccessKey] =
                    successMessage;

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        reservationId
                    });
            }

            if (result.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                return await SignOutAndRedirectToLoginAsync();
            }

            if (result.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (result.StatusCode ==
                HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (result.StatusCode ==
                HttpStatusCode.Conflict)
            {
                TempData[ReservationErrorKey] =
                    string.IsNullOrWhiteSpace(result.Detail)
                        ? conflictFallbackMessage
                        : result.Detail;

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        reservationId
                    });
            }

            return View(
                ReservationErrorViewName);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while attempting to {OperationName} reservation {ReservationId}.",
                operationName,
                reservationId);

            return View(
                ReservationErrorViewName);
        }
    }

    /// <summary>
    /// Handles adding another vehicle to the current
    /// reservation draft.
    /// </summary>
    /// <param name="viewModel">
    /// The reservation draft being edited.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The rebuilt reservation creation page.
    /// </returns>
    private async Task<IActionResult> HandleAddVehicleAsync(
        CreateReservationViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var vehicleAdded =
            TryAddVehicleToDraft(
                viewModel);

        return await ReloadCreateViewAsync(
            viewModel,
            cancellationToken,
            clearModelState: vehicleAdded);
    }

    /// <summary>
    /// Handles removing a vehicle from the current
    /// reservation draft.
    /// </summary>
    /// <param name="viewModel">
    /// The reservation draft being edited.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The rebuilt reservation creation page.
    /// </returns>
    private async Task<IActionResult> HandleRemoveVehicleAsync(
        CreateReservationViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var vehicleRemoved =
            TryRemoveVehicleFromDraft(
                viewModel);

        return await ReloadCreateViewAsync(
            viewModel,
            cancellationToken,
            clearModelState: vehicleRemoved);
    }

    /// <summary>
    /// Attempts to add another vehicle to the
    /// current reservation draft.
    /// </summary>
    /// <param name="viewModel">
    /// The reservation draft being edited.
    /// </param>
    /// <returns>
    /// True when the vehicle is added successfully;
    /// otherwise, false.
    /// </returns>
    private bool TryAddVehicleToDraft(
        CreateReservationViewModel viewModel)
    {
        var vehicleId =
            viewModel.VehicleToAddId;

        if (!vehicleId.HasValue ||
            vehicleId.Value <= 0)
        {
            ModelState.AddModelError(
                nameof(viewModel.VehicleToAddId),
                "Select a valid vehicle.");

            return false;
        }

        if (viewModel.Vehicles.Any(vehicle =>
            vehicle.VehicleId ==
            vehicleId.Value))
        {
            ModelState.AddModelError(
                nameof(viewModel.VehicleToAddId),
                "This vehicle is already included in the reservation.");

            return false;
        }

        viewModel.Vehicles.Add(
            new CreateReservationVehicleViewModel
            {
                VehicleId =
                    vehicleId.Value
            });

        viewModel.VehicleToAddId = null;

        return true;
    }

    /// <summary>
    /// Attempts to remove a vehicle from the
    /// current reservation draft.
    /// </summary>
    /// <param name="viewModel">
    /// The reservation draft being edited.
    /// </param>
    /// <returns>
    /// True when the vehicle is removed successfully;
    /// otherwise, false.
    /// </returns>
    private bool TryRemoveVehicleFromDraft(
        CreateReservationViewModel viewModel)
    {
        var vehicleId =
            viewModel.VehicleToRemoveId;

        if (!vehicleId.HasValue ||
            vehicleId.Value <= 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "Select a valid vehicle to remove.");

            return false;
        }

        if (viewModel.Vehicles.Count <= 1)
        {
            ModelState.AddModelError(
                string.Empty,
                "A reservation must contain at least one vehicle.");

            return false;
        }

        var vehicleIndex =
            viewModel.Vehicles.FindIndex(vehicle =>
                vehicle.VehicleId ==
                vehicleId.Value);

        if (vehicleIndex < 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "The selected vehicle could not be removed.");

            return false;
        }

        viewModel.Vehicles.RemoveAt(
            vehicleIndex);

        viewModel.VehicleToRemoveId = null;

        return true;
    }

    /// <summary>
    /// Validates the reservation draft before
    /// it is submitted to the API.
    /// </summary>
    /// <param name="viewModel">
    /// The reservation draft to validate.
    /// </param>
    /// <returns>
    /// True when the draft is valid; otherwise, false.
    /// </returns>
    private bool ValidateReservationDraft(
        CreateReservationViewModel viewModel)
    {
        if (viewModel.Vehicles.Count == 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "At least one vehicle is required.");
        }

        var hasDuplicateVehicles =
            viewModel.Vehicles
                .GroupBy(vehicle =>
                    vehicle.VehicleId)
                .Any(group =>
                    group.Count() > 1);

        if (hasDuplicateVehicles)
        {
            ModelState.AddModelError(
                string.Empty,
                "A vehicle can only be included once in a reservation.");
        }

        return ModelState.IsValid;
    }

    /// <summary>
    /// Rebuilds the reservation creation model
    /// and returns the creation page.
    /// </summary>
    /// <param name="viewModel">
    /// The reservation draft to rebuild.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <param name="clearModelState">
    /// Indicates whether posted values should be cleared
    /// after the draft structure changes.
    /// </param>
    /// <returns>
    /// The reservation creation page or the error page
    /// when required catalog information cannot be loaded.
    /// </returns>
    private async Task<IActionResult> ReloadCreateViewAsync(
        CreateReservationViewModel viewModel,
        CancellationToken cancellationToken,
        bool clearModelState = false)
    {
        var loaded =
            await PopulateCreateModelAsync(
                viewModel,
                cancellationToken);

        if (!loaded)
        {
            return View(
                ReservationErrorViewName);
        }

        if (clearModelState)
        {
            ModelState.Clear();
        }

        return View(
            CreateViewName,
            viewModel);
    }

    /// <summary>
    /// Submits a completed reservation draft
    /// to the DriveFleet API.
    /// </summary>
    /// <param name="viewModel">
    /// The completed reservation draft.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A redirect to the reservation details page when successful,
    /// or an appropriate error result.
    /// </returns>
    private async Task<IActionResult> SubmitReservationAsync(
        CreateReservationViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var accessToken =
            await GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return await SignOutAndRedirectToLoginAsync();
        }

        var vehicles =
            MapCreateReservationVehicles(
                viewModel);

        var result =
            await _reservationApiClient.CreateAsync(
                accessToken,
                vehicles,
                cancellationToken);

        if (result.StatusCode == HttpStatusCode.Created &&
            result.Reservation is not null)
        {
            TempData[ReservationSuccessKey] =
                "The reservation was created successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    reservationId =
                        result.Reservation.ReservationId
                });
        }

        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            return await SignOutAndRedirectToLoginAsync();
        }

        if (result.StatusCode == HttpStatusCode.Forbidden)
        {
            return Forbid();
        }

        if (result.StatusCode == HttpStatusCode.BadRequest ||
            result.StatusCode == HttpStatusCode.Conflict)
        {
            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(
                    result.Detail)
                    ? "The reservation information is invalid."
                    : result.Detail);

            return await ReloadCreateViewAsync(
                viewModel,
                cancellationToken);
        }

        return View(
            ReservationErrorViewName);
    }

    /// <summary>
    /// Maps the reservation creation form to the vehicle
    /// requests expected by the API client.
    /// </summary>
    /// <param name="viewModel">
    /// The reservation creation form.
    /// </param>
    /// <returns>
    /// The vehicles and selected extras to send to the API.
    /// </returns>
    private static IReadOnlyCollection<CreateReservationVehicleApiModel>
        MapCreateReservationVehicles(
            CreateReservationViewModel viewModel)
    {
        return viewModel.Vehicles
            .Select(vehicle =>
                new CreateReservationVehicleApiModel
                {
                    VehicleId = vehicle.VehicleId,
                    StartDate = vehicle.StartDate,
                    EndDate = vehicle.EndDate,

                    Extras = vehicle.Extras
                        .Where(extra =>
                            extra.Quantity > 0)
                        .Select(extra =>
                            new CreateReservationExtraApiModel
                            {
                                ExtraId = extra.ExtraId,
                                Quantity = extra.Quantity
                            })
                        .ToList()
                })
            .ToList();
    }

    /// <summary>
    /// Loads the catalog information required by the reservation
    /// creation form while preserving vehicles already selected.
    /// </summary>
    /// <param name="viewModel">
    /// The reservation creation model to populate.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// True when the required catalog information is loaded successfully;
    /// otherwise, false.
    /// </returns>
    private async Task<bool> PopulateCreateModelAsync(
        CreateReservationViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var vehiclesTask =
            _vehicleApiClient.GetAllAsync(
                cancellationToken);

        var extrasTask =
            _extraApiClient.GetActiveAsync(
                cancellationToken);

        await Task.WhenAll(
            vehiclesTask,
            extrasTask);

        var vehiclesResult =
            await vehiclesTask;

        var extrasResult =
            await extrasTask;

        if (vehiclesResult.StatusCode != HttpStatusCode.OK ||
            extrasResult.StatusCode != HttpStatusCode.OK)
        {
            return false;
        }

        var reservableVehicles =
            vehiclesResult.Vehicles
                .Where(vehicle =>
                    !string.Equals(
                        vehicle.VehicleStatusName,
                        UnavailableVehicleStatusName,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

        var reservableVehiclesById =
            reservableVehicles
                .ToDictionary(vehicle =>
                    vehicle.VehicleId);

        var postedVehicles =
            viewModel.Vehicles
                .GroupBy(vehicle =>
                    vehicle.VehicleId)
                .Select(group =>
                    group.First())
                .ToList();

        var selectedVehicleIds =
            postedVehicles
                .Select(vehicle =>
                    vehicle.VehicleId)
                .ToHashSet();

        var defaultStartDate =
            DateTime.Today
                .AddDays(1)
                .AddHours(
                    DefaultRentalStartHour);

        var defaultEndDate =
            defaultStartDate.AddDays(
                DefaultRentalDurationDays);

        var populatedVehicles =
            new List<CreateReservationVehicleViewModel>();

        foreach (var postedVehicle in postedVehicles)
        {
            if (!reservableVehiclesById.TryGetValue(
                postedVehicle.VehicleId,
                out var vehicle))
            {
                return false;
            }

            var postedExtrasById =
                postedVehicle.Extras
                    .GroupBy(extra =>
                        extra.ExtraId)
                    .Select(group =>
                        group.First())
                    .ToDictionary(extra =>
                        extra.ExtraId);

            populatedVehicles.Add(
                new CreateReservationVehicleViewModel
                {
                    VehicleId = vehicle.VehicleId,
                    Brand = vehicle.Brand,
                    Model = vehicle.Model,
                    LicensePlate = vehicle.LicensePlate,
                    DailyPrice = vehicle.DailyPrice,

                    StartDate =
                        postedVehicle.StartDate == default
                            ? defaultStartDate
                            : postedVehicle.StartDate,

                    EndDate =
                        postedVehicle.EndDate == default
                            ? defaultEndDate
                            : postedVehicle.EndDate,

                    Extras =
                        extrasResult.Extras
                            .Select(extra =>
                            {
                                postedExtrasById.TryGetValue(
                                    extra.ExtraId,
                                    out var postedExtra);

                                return new CreateReservationExtraViewModel
                                {
                                    ExtraId = extra.ExtraId,
                                    Name = extra.Name,
                                    Price = extra.Price,
                                    Quantity =
                                        postedExtra?.Quantity ?? 0
                                };
                            })
                            .ToList()
                });
        }

        viewModel.Vehicles =
            populatedVehicles;

        viewModel.AvailableVehicles =
            reservableVehicles
                .Where(vehicle =>
                    !selectedVehicleIds.Contains(
                        vehicle.VehicleId))
                .OrderBy(vehicle =>
                    vehicle.Brand)
                .ThenBy(vehicle =>
                    vehicle.Model)
                .Select(vehicle =>
                {
                    var orderedPhotos =
                        vehicle.Photos
                            .OrderBy(photo =>
                                photo.DisplayOrder)
                            .ToList();

                    var mainPhoto =
                        orderedPhotos.FirstOrDefault(photo =>
                            photo.DisplayOrder ==
                            MainPhotoDisplayOrder)
                        ?? orderedPhotos.FirstOrDefault();

                    return new ReservationVehicleOptionViewModel
                    {
                        VehicleId = vehicle.VehicleId,
                        Brand = vehicle.Brand,
                        Model = vehicle.Model,
                        Year = vehicle.Year,
                        LicensePlate = vehicle.LicensePlate,
                        Seats = vehicle.Seats,
                        CategoryName = vehicle.CategoryName,
                        DailyPrice = vehicle.DailyPrice,

                        MainPhotoUrl =
                            _vehicleApiClient.GetPublicResourceUrl(
                                mainPhoto?.FilePath)
                    };
                })
                .ToList();

        return true;
    }

    /// <summary>
    /// Calculates the date range represented by the
    /// selected calendar view.
    /// </summary>
    /// <param name="referenceDate">
    /// The date used as the calendar reference point.
    /// </param>
    /// <param name="viewMode">
    /// The selected calendar view.
    /// </param>
    /// <returns>
    /// The inclusive start and end dates of the period.
    /// </returns>
    private static (DateTime StartDate, DateTime EndDate)
        GetCalendarPeriod(
            DateTime referenceDate,
            string viewMode)
    {
        if (string.Equals(
            viewMode,
            CalendarWeekView,
            StringComparison.OrdinalIgnoreCase))
        {
            var daysSinceMonday =
                ((int)referenceDate.DayOfWeek + 6) % 7;

            var startDate =
                referenceDate.AddDays(
                    -daysSinceMonday);

            return (
                startDate,
                startDate.AddDays(6));
        }

        var monthStart =
            new DateTime(
                referenceDate.Year,
                referenceDate.Month,
                1);

        return (
            monthStart,
            monthStart
                .AddMonths(1)
                .AddDays(-1));
    }

    /// <summary>
    /// Maps reservation API models to calendar entries.
    /// </summary>
    /// <param name="reservations">
    /// The reservations returned by the API.
    /// </param>
    /// <returns>
    /// Calendar entries representing reserved vehicles.
    /// </returns>
    private static IReadOnlyList<ReservationCalendarEntryViewModel>
        MapCalendarEntries(
            IEnumerable<ReservationApiModel> reservations)
    {
        return reservations
            .Where(reservation =>
                !string.Equals(
                    reservation.ReservationStatusName,
                    CancelledReservationStatusName,
                    StringComparison.OrdinalIgnoreCase))
            .SelectMany(reservation =>
                reservation.Vehicles.Select(vehicle =>
                    new ReservationCalendarEntryViewModel
                    {
                        ReservationId =
                            reservation.ReservationId,

                        UserFullName =
                            reservation.UserFullName,

                        ReservationStatusName =
                            reservation.ReservationStatusName,

                        VehicleBrand =
                            vehicle.VehicleBrand,

                        VehicleModel =
                            vehicle.VehicleModel,

                        LicensePlate =
                            vehicle.LicensePlate,

                        StartDate =
                            vehicle.StartDate,

                        EndDate =
                            vehicle.EndDate
                    }))
            .OrderBy(entry =>
                entry.StartDate)
            .ThenBy(entry =>
                entry.VehicleBrand)
            .ThenBy(entry =>
                entry.VehicleModel)
            .ToList();
    }

    /// <summary>
    /// Gets the JWT access token stored in the authentication session.
    /// </summary>
    /// <returns>
    /// The access token when available; otherwise, null.
    /// </returns>
    private Task<string?> GetAccessTokenAsync()
    {
        return HttpContext.GetTokenAsync(
            AccessTokenName);
    }

    /// <summary>
    /// Signs out the current Web session
    /// and redirects to the login page.
    /// </summary>
    /// <returns>
    /// A redirect to the account login page.
    /// </returns>
    private async Task<IActionResult> SignOutAndRedirectToLoginAsync()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(
            "Login",
            "Account");
    }

    /// <summary>
    /// Determines whether the authenticated user can manage
    /// reservations belonging to all clients.
    /// </summary>
    /// <returns>
    /// True for administrators and employees; otherwise, false.
    /// </returns>
    private bool CanManageAllReservations()
    {
        return User.IsInRole(
                   AdminRole) ||
               User.IsInRole(
                   EmployeeRole);
    }

    /// <summary>
    /// Maps an API reservation model to the model
    /// displayed by the Web project.
    /// </summary>
    /// <param name="reservation">
    /// The API reservation model.
    /// </param>
    /// <returns>
    /// The mapped reservation view model.
    /// </returns>
    private static ReservationViewModel MapReservation(
        ReservationApiModel reservation)
    {
        return new ReservationViewModel
        {
            ReservationId = reservation.ReservationId,
            UserId = reservation.UserId,
            UserFullName = reservation.UserFullName,
            UserEmail = reservation.UserEmail,

            ReservationStatusId =
                reservation.ReservationStatusId,

            ReservationStatusName =
                reservation.ReservationStatusName,

            TotalValue = reservation.TotalValue,
            CreatedAt = reservation.CreatedAt,
            UpdatedAt = reservation.UpdatedAt,

            Vehicles =
                reservation.Vehicles
                    .Select(
                        MapReservationVehicle)
                    .ToList()
        };
    }

    /// <summary>
    /// Maps an API reservation vehicle model
    /// to its Web view model.
    /// </summary>
    /// <param name="vehicle">
    /// The API reservation vehicle model.
    /// </param>
    /// <returns>
    /// The mapped reservation vehicle view model.
    /// </returns>
    private static ReservationVehicleViewModel MapReservationVehicle(
        ReservationVehicleApiModel vehicle)
    {
        return new ReservationVehicleViewModel
        {
            ReservationVehicleId =
                vehicle.ReservationVehicleId,

            VehicleId = vehicle.VehicleId,
            VehicleBrand = vehicle.VehicleBrand,
            VehicleModel = vehicle.VehicleModel,
            LicensePlate = vehicle.LicensePlate,
            StartDate = vehicle.StartDate,
            EndDate = vehicle.EndDate,
            DailyPrice = vehicle.DailyPrice,
            RentalDays = vehicle.RentalDays,
            RentalValue = vehicle.RentalValue,
            ExtrasValue = vehicle.ExtrasValue,
            TotalValue = vehicle.TotalValue,

            Extras =
                vehicle.Extras
                    .Select(
                        MapReservationExtra)
                    .ToList()
        };
    }

    /// <summary>
    /// Maps an API reservation extra model
    /// to its Web view model.
    /// </summary>
    /// <param name="extra">
    /// The API reservation extra model.
    /// </param>
    /// <returns>
    /// The mapped reservation extra view model.
    /// </returns>
    private static ReservationExtraViewModel MapReservationExtra(
        ReservationExtraApiModel extra)
    {
        return new ReservationExtraViewModel
        {
            ExtraId = extra.ExtraId,
            Name = extra.Name,
            Quantity = extra.Quantity,
            Price = extra.Price,
            Total = extra.Total
        };
    }
}
