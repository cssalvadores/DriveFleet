using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DriveFleet.Web.Services;

/// <summary>
/// Generates PDF reports for DriveFleet backoffice operations.
/// </summary>
public class PdfReportService
{
    private static readonly CultureInfo PortugueseCulture =
        CultureInfo.GetCultureInfo(
            "pt-PT");

    /// <summary>
    /// Configures the QuestPDF license used by the application.
    /// </summary>
    static PdfReportService()
    {
        QuestPDF.Settings.License =
            LicenseType.Community;
    }

    /// <summary>
    /// Generates a PDF report containing reservation information.
    /// </summary>
    /// <param name="reservations">
    /// Reservations included in the report.
    /// </param>
    /// <param name="fromDate">
    /// Optional report start date.
    /// </param>
    /// <param name="toDate">
    /// Optional report end date.
    /// </param>
    /// <returns>
    /// The generated PDF document as a byte array.
    /// </returns>
    public byte[] GenerateReservationsReport(
        IReadOnlyList<ReservationApiModel> reservations,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var generatedAt =
            DateTime.Now;

        var totalRevenue =
            reservations.Sum(
                reservation =>
                    reservation.TotalValue);

        var document =
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(
                        PageSizes.A4.Landscape());

                    page.Margin(30);

                    page.PageColor(
                        Colors.White);

                    page.DefaultTextStyle(
                        style =>
                            style.FontSize(8));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("DriveFleet")
                                .FontSize(22)
                                .SemiBold()
                                .FontColor(
                                    Colors.Blue.Medium);

                            column.Item()
                                .PaddingTop(2)
                                .Text(
                                    "Reservations Report")
                                .FontSize(14)
                                .SemiBold();

                            column.Item()
                                .PaddingTop(4)
                                .Text(
                                    BuildPeriodText(
                                        fromDate,
                                        toDate))
                                .FontSize(8)
                                .FontColor(
                                    Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(15)
                        .Column(column =>
                        {
                            column.Spacing(12);

                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Element(
                                            container =>
                                                SummaryCard(
                                                    container,
                                                    "Reservations",
                                                    reservations.Count
                                                        .ToString()));

                                    row.ConstantItem(15);

                                    row.RelativeItem()
                                        .Element(
                                            container =>
                                                SummaryCard(
                                                    container,
                                                    "Total value",
                                                    totalRevenue.ToString(
                                                        "C2",
                                                        PortugueseCulture)));

                                    row.ConstantItem(15);

                                    row.RelativeItem()
                                        .Element(
                                            container =>
                                                SummaryCard(
                                                    container,
                                                    "Generated",
                                                    generatedAt.ToString(
                                                        "dd/MM/yyyy HH:mm")));
                                });

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(
                                        columns =>
                                        {
                                            columns.ConstantColumn(45);
                                            columns.RelativeColumn(1.7f);
                                            columns.RelativeColumn(1.7f);
                                            columns.RelativeColumn(1.2f);
                                            columns.RelativeColumn(2.4f);
                                            columns.RelativeColumn(1.8f);
                                            columns.RelativeColumn(1.1f);
                                        });

                                    table.Header(header =>
                                    {
                                        HeaderCell(
                                            header,
                                            "ID");

                                        HeaderCell(
                                            header,
                                            "Customer");

                                        HeaderCell(
                                            header,
                                            "Email");

                                        HeaderCell(
                                            header,
                                            "Status");

                                        HeaderCell(
                                            header,
                                            "Vehicles");

                                        HeaderCell(
                                            header,
                                            "Rental period");

                                        HeaderCell(
                                            header,
                                            "Total");
                                    });

                                    foreach (var reservation
                                             in reservations)
                                    {
                                        var vehicles =
                                            reservation.Vehicles
                                                .ToList();

                                        var vehicleNames =
                                            vehicles.Count == 0
                                                ? "-"
                                                : string.Join(
                                                    ", ",
                                                    vehicles.Select(
                                                        vehicle =>
                                                            $"{vehicle.VehicleBrand} " +
                                                            $"{vehicle.VehicleModel}"));

                                        var rentalPeriod =
                                            BuildRentalPeriod(
                                                vehicles);

                                        BodyCell(
                                            table,
                                            reservation
                                                .ReservationId
                                                .ToString());

                                        BodyCell(
                                            table,
                                            reservation
                                                .UserFullName);

                                        BodyCell(
                                            table,
                                            reservation
                                                .UserEmail);

                                        BodyCell(
                                            table,
                                            reservation
                                                .ReservationStatusName);

                                        BodyCell(
                                            table,
                                            vehicleNames);

                                        BodyCell(
                                            table,
                                            rentalPeriod);

                                        BodyCell(
                                            table,
                                            reservation
                                                .TotalValue
                                                .ToString(
                                                    "C2",
                                                    PortugueseCulture));
                                    }
                                });

                            if (reservations.Count == 0)
                            {
                                column.Item()
                                    .PaddingTop(15)
                                    .AlignCenter()
                                    .Text(
                                        "No reservations were found for the selected period.")
                                    .FontColor(
                                        Colors.Grey.Darken1);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(
                                "DriveFleet · Page ");

                            text.CurrentPageNumber();

                            text.Span(
                                " of ");

                            text.TotalPages();
                        });
                });
            });

        return document.GeneratePdf();
    }

    /// <summary>
    /// Generates a PDF report containing the current vehicle inventory.
    /// </summary>
    /// <param name="vehicles">
    /// Vehicles included in the inventory.
    /// </param>
    /// <returns>
    /// The generated PDF document as a byte array.
    /// </returns>
    public byte[] GenerateVehicleInventoryReport(
        IReadOnlyList<VehicleApiModel> vehicles)
    {
        var generatedAt =
            DateTime.Now;

        var availableCount =
            vehicles.Count(vehicle =>
                string.Equals(
                    vehicle.VehicleStatusName,
                    "Available",
                    StringComparison.OrdinalIgnoreCase));

        var unavailableCount =
            vehicles.Count(vehicle =>
                string.Equals(
                    vehicle.VehicleStatusName,
                    "Unavailable",
                    StringComparison.OrdinalIgnoreCase));

        var document =
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(
                        PageSizes.A4.Landscape());

                    page.Margin(30);

                    page.PageColor(
                        Colors.White);

                    page.DefaultTextStyle(
                        style =>
                            style.FontSize(8));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("DriveFleet")
                                .FontSize(22)
                                .SemiBold()
                                .FontColor(
                                    Colors.Blue.Medium);

                            column.Item()
                                .PaddingTop(2)
                                .Text(
                                    "Vehicle Inventory Report")
                                .FontSize(14)
                                .SemiBold();

                            column.Item()
                                .PaddingTop(4)
                                .Text(
                                    $"Generated on {generatedAt:dd/MM/yyyy HH:mm}")
                                .FontSize(8)
                                .FontColor(
                                    Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(15)
                        .Column(column =>
                        {
                            column.Spacing(12);

                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Element(
                                            container =>
                                                SummaryCard(
                                                    container,
                                                    "Total vehicles",
                                                    vehicles.Count
                                                        .ToString()));

                                    row.ConstantItem(15);

                                    row.RelativeItem()
                                        .Element(
                                            container =>
                                                SummaryCard(
                                                    container,
                                                    "Available",
                                                    availableCount
                                                        .ToString()));

                                    row.ConstantItem(15);

                                    row.RelativeItem()
                                        .Element(
                                            container =>
                                                SummaryCard(
                                                    container,
                                                    "Unavailable",
                                                    unavailableCount
                                                        .ToString()));
                                });

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(
                                        columns =>
                                        {
                                            columns.ConstantColumn(45);
                                            columns.RelativeColumn(1.3f);
                                            columns.RelativeColumn(1.3f);
                                            columns.RelativeColumn(1.2f);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(0.8f);
                                            columns.RelativeColumn(1.2f);
                                            columns.RelativeColumn(1.1f);
                                        });

                                    table.Header(header =>
                                    {
                                        HeaderCell(
                                            header,
                                            "ID");

                                        HeaderCell(
                                            header,
                                            "Brand");

                                        HeaderCell(
                                            header,
                                            "Model");

                                        HeaderCell(
                                            header,
                                            "Plate");

                                        HeaderCell(
                                            header,
                                            "Category");

                                        HeaderCell(
                                            header,
                                            "Year");

                                        HeaderCell(
                                            header,
                                            "Status");

                                        HeaderCell(
                                            header,
                                            "Daily price");
                                    });

                                    foreach (var vehicle
                                             in vehicles)
                                    {
                                        BodyCell(
                                            table,
                                            vehicle.VehicleId
                                                .ToString());

                                        BodyCell(
                                            table,
                                            vehicle.Brand);

                                        BodyCell(
                                            table,
                                            vehicle.Model);

                                        BodyCell(
                                            table,
                                            vehicle.LicensePlate);

                                        BodyCell(
                                            table,
                                            vehicle.CategoryName);

                                        BodyCell(
                                            table,
                                            vehicle.Year
                                                .ToString());

                                        BodyCell(
                                            table,
                                            vehicle.VehicleStatusName);

                                        BodyCell(
                                            table,
                                            vehicle.DailyPrice
                                                .ToString(
                                                    "C2",
                                                    PortugueseCulture));
                                    }
                                });

                            if (vehicles.Count == 0)
                            {
                                column.Item()
                                    .PaddingTop(15)
                                    .AlignCenter()
                                    .Text(
                                        "No vehicles are currently available in the inventory.")
                                    .FontColor(
                                        Colors.Grey.Darken1);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(
                                "DriveFleet · Page ");

                            text.CurrentPageNumber();

                            text.Span(
                                " of ");

                            text.TotalPages();
                        });
                });
            });

        return document.GeneratePdf();
    }

    /// <summary>
    /// Builds a report period description.
    /// </summary>
    private static string BuildPeriodText(
        DateTime? fromDate,
        DateTime? toDate)
    {
        if (fromDate.HasValue &&
            toDate.HasValue)
        {
            return
                $"Period: {fromDate.Value:dd/MM/yyyy} " +
                $"to {toDate.Value:dd/MM/yyyy}";
        }

        if (fromDate.HasValue)
        {
            return
                $"From {fromDate.Value:dd/MM/yyyy}";
        }

        if (toDate.HasValue)
        {
            return
                $"Until {toDate.Value:dd/MM/yyyy}";
        }

        return
            "All reservation records";
    }

    /// <summary>
    /// Builds the rental period shown in a reservation report.
    /// </summary>
    private static string BuildRentalPeriod(
        IReadOnlyList<ReservationVehicleApiModel> vehicles)
    {
        if (vehicles.Count == 0)
        {
            return "-";
        }

        var startDate =
            vehicles.Min(vehicle =>
                vehicle.StartDate);

        var endDate =
            vehicles.Max(vehicle =>
                vehicle.EndDate);

        return
            $"{startDate:dd/MM/yyyy} - " +
            $"{endDate:dd/MM/yyyy}";
    }

    /// <summary>
    /// Creates a summary card used at the top of a report.
    /// </summary>
    private static void SummaryCard(
        IContainer container,
        string label,
        string value)
    {
        container
            .Border(1)
            .BorderColor(
                Colors.Grey.Lighten2)
            .Background(
                Colors.Grey.Lighten4)
            .Padding(10)
            .Column(column =>
            {
                column.Item()
                    .Text(label)
                    .FontSize(7)
                    .FontColor(
                        Colors.Grey.Darken1);

                column.Item()
                    .PaddingTop(3)
                    .Text(value)
                    .FontSize(12)
                    .SemiBold()
                    .FontColor(
                        Colors.Blue.Darken2);
            });
    }

    /// <summary>
    /// Creates a table header cell.
    /// </summary>
    private static void HeaderCell(
        TableCellDescriptor header,
        string text)
    {
        header.Cell()
            .Background(
                Colors.Blue.Darken2)
            .PaddingVertical(6)
            .PaddingHorizontal(5)
            .Text(text)
            .FontColor(
                Colors.White)
            .SemiBold();
    }

    /// <summary>
    /// Creates a standard table body cell.
    /// </summary>
    private static void BodyCell(
        TableDescriptor table,
        string? text)
    {
        table.Cell()
            .BorderBottom(1)
            .BorderColor(
                Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .PaddingHorizontal(5)
            .Text(
                string.IsNullOrWhiteSpace(text)
                    ? "-"
                    : text);
    }
}
