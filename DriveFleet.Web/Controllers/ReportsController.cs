using System.Net;
using DriveFleet.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveFleet.Web.Controllers;

/// <summary>
/// Provides administrative reporting operations.
/// </summary>
[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    private const string AccessTokenName =
        "access_token";

    private readonly ReservationApiClient
        _reservationApiClient;

    private readonly VehicleApiClient
        _vehicleApiClient;

    private readonly PdfReportService
    _pdfReportService;

    private readonly XmlExportService
        _xmlExportService;

    private readonly ILogger<ReportsController>
        _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReportsController"/> class.
    /// </summary>
    public ReportsController(
        ReservationApiClient reservationApiClient,
        VehicleApiClient vehicleApiClient,
        ILogger<ReportsController> logger)
    {
        _reservationApiClient =
            reservationApiClient;

        _vehicleApiClient =
            vehicleApiClient;

        _pdfReportService =
            new PdfReportService();

        _xmlExportService =
            new XmlExportService();

        _logger =
            logger;
    }

    /// <summary>
    /// Displays the administrative reporting page.
    /// </summary>
    /// <returns>
    /// The reporting page for the authenticated administrator.
    /// </returns>
    [HttpGet("/reports")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Generates an administrative reservation report
    /// for the selected period.
    /// </summary>
    /// <param name="fromDate">
    /// Optional first date included in the report.
    /// </param>
    /// <param name="toDate">
    /// Optional last date included in the report.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if needed.
    /// </param>
    /// <returns>
    /// A PDF file containing reservation information.
    /// </returns>
    [HttpGet("/reports/reservations/pdf")]
    public async Task<IActionResult> ReservationsPdf(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        if (fromDate.HasValue &&
            toDate.HasValue &&
            fromDate.Value.Date >
            toDate.Value.Date)
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
            var result =
                await _reservationApiClient
                    .GetAllAsync(
                        accessToken,
                        statusId: null,
                        fromDate,
                        toDate,
                        cancellationToken);

            if (result.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                return await
                    SignOutAndRedirectToLoginAsync();
            }

            if (result.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (result.StatusCode !=
                HttpStatusCode.OK)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    "The reservation report data could not be retrieved.");
            }

            var pdfBytes =
                _pdfReportService
                    .GenerateReservationsReport(
                        result.Reservations,
                        fromDate,
                        toDate);

            var fileName =
                BuildReservationsFileName(
                    fromDate,
                    toDate);

            return File(
                pdfBytes,
                "application/pdf",
                fileName);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while generating the reservations PDF report.");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "The reporting service is temporarily unavailable.");
        }
    }

    /// <summary>
    /// Generates a structured XML export containing
    /// reservation information for the selected period.
    /// </summary>
    /// <param name="fromDate">
    /// Optional first date included in the export.
    /// </param>
    /// <param name="toDate">
    /// Optional last date included in the export.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if needed.
    /// </param>
    /// <returns>
    /// An XML file containing reservation information.
    /// </returns>
    [HttpGet("/reports/reservations/xml")]
    public async Task<IActionResult> ReservationsXml(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        if (fromDate.HasValue &&
            toDate.HasValue &&
            fromDate.Value.Date >
            toDate.Value.Date)
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
            var result =
                await _reservationApiClient
                    .GetAllAsync(
                        accessToken,
                        statusId: null,
                        fromDate: fromDate,
                        toDate: toDate,
                        cancellationToken:
                            cancellationToken);

            if (result.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                return await
                    SignOutAndRedirectToLoginAsync();
            }

            if (result.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (result.StatusCode !=
                HttpStatusCode.OK)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    "The reservation export data could not be retrieved.");
            }

            var xmlBytes =
                _xmlExportService
                    .GenerateReservationsExport(
                        result.Reservations,
                        fromDate,
                        toDate);

            var fileName =
                BuildReservationsXmlFileName(
                    fromDate,
                    toDate);

            return File(
                xmlBytes,
                "application/xml",
                fileName);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while generating the reservations XML export.");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "The export service is temporarily unavailable.");
        }
    }

    /// <summary>
    /// Generates the current vehicle inventory report.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if needed.
    /// </param>
    /// <returns>
    /// A PDF file containing the current vehicle inventory.
    /// </returns>
    [HttpGet("/reports/vehicles/pdf")]
    public async Task<IActionResult> VehiclesPdf(
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _vehicleApiClient
                    .GetAllAsync(
                        cancellationToken);

            if (result.StatusCode !=
                HttpStatusCode.OK)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    "The vehicle inventory data could not be retrieved.");
            }

            var pdfBytes =
                _pdfReportService
                    .GenerateVehicleInventoryReport(
                        result.Vehicles);

            var fileName =
                $"drivefleet-vehicle-inventory-" +
                $"{DateTime.Now:yyyy-MM-dd}.pdf";

            return File(
                pdfBytes,
                "application/pdf",
                fileName);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while generating the vehicle inventory PDF report.");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "The reporting service is temporarily unavailable.");
        }
    }

    /// <summary>
    /// Generates a structured XML export containing
    /// the current DriveFleet vehicle inventory.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the request if needed.
    /// </param>
    /// <returns>
    /// An XML file containing vehicle information.
    /// </returns>
    [HttpGet("/reports/vehicles/xml")]
    public async Task<IActionResult> VehiclesXml(
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _vehicleApiClient
                    .GetAllAsync(
                        cancellationToken);

            if (result.StatusCode !=
                HttpStatusCode.OK)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    "The vehicle export data could not be retrieved.");
            }

            var xmlBytes =
                _xmlExportService
                    .GenerateVehiclesExport(
                        result.Vehicles);

            var fileName =
                $"drivefleet-vehicles-" +
                $"{DateTime.Now:yyyy-MM-dd}.xml";

            return File(
                xmlBytes,
                "application/xml",
                fileName);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Unable to communicate with the DriveFleet API while generating the vehicle XML export.");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "The export service is temporarily unavailable.");
        }
    }

    /// <summary>
    /// Builds the reservation report download filename.
    /// </summary>
    private static string BuildReservationsFileName(
        DateTime? fromDate,
        DateTime? toDate)
    {
        if (fromDate.HasValue &&
            toDate.HasValue)
        {
            return
                $"drivefleet-reservations-" +
                $"{fromDate.Value:yyyy-MM-dd}-" +
                $"{toDate.Value:yyyy-MM-dd}.pdf";
        }

        return
            $"drivefleet-reservations-" +
            $"{DateTime.Now:yyyy-MM-dd}.pdf";
    }

    /// <summary>
    /// Builds the reservation XML export filename.
    /// </summary>
    private static string BuildReservationsXmlFileName(
        DateTime? fromDate,
        DateTime? toDate)
    {
        if (fromDate.HasValue &&
            toDate.HasValue)
        {
            return
                $"drivefleet-reservations-" +
                $"{fromDate.Value:yyyy-MM-dd}-" +
                $"{toDate.Value:yyyy-MM-dd}.xml";
        }

        return
            $"drivefleet-reservations-" +
            $"{DateTime.Now:yyyy-MM-dd}.xml";
    }

    /// <summary>
    /// Clears the local authentication cookie
    /// and redirects the user to the login page.
    /// </summary>
    private async Task<IActionResult>SignOutAndRedirectToLoginAsync()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme);

        return RedirectToAction(
            "Login",
            "Account");
    }
}
