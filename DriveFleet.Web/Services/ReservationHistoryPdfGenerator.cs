using System.Globalization;
using DriveFleet.Web.Models.Reservations;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DriveFleet.Web.Services;

/// <summary>
/// Generates professional PDF documents containing
/// client reservation history.
/// </summary>
public static class ReservationHistoryPdfGenerator
{
    private static readonly CultureInfo PortugueseCulture =
        CultureInfo.GetCultureInfo(
            "pt-PT");

    static ReservationHistoryPdfGenerator()
    {
        QuestPDF.Settings.License =
            LicenseType.Community;
    }

    /// <summary>
    /// Generates a reservation history PDF.
    /// </summary>
    /// <param name="reservations">
    /// The completed and cancelled reservations
    /// to include in the document.
    /// </param>
    /// <returns>
    /// The generated PDF bytes.
    /// </returns>
    public static byte[] Generate(
        IReadOnlyList<ReservationViewModel> reservations)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(
                    PageSizes.A4);

                page.Margin(35);

                page.DefaultTextStyle(style =>
                    style.FontSize(9));

                page.Header()
                    .Element(
                        ComposeHeader);

                page.Content()
                    .PaddingVertical(20)
                    .Column(column =>
                    {
                        column.Spacing(20);

                        if (reservations.Count == 0)
                        {
                            column.Item()
                                .Text(
                                    "No completed or cancelled reservations were found.");

                            return;
                        }

                        foreach (var reservation in reservations)
                        {
                            column.Item()
                                .Element(container =>
                                    ComposeReservation(
                                        container,
                                        reservation));
                        }
                    });

                page.Footer()
                    .Element(
                        ComposeFooter);
            });
        }).GeneratePdf();
    }

    /// <summary>
    /// Builds the document header.
    /// </summary>
    private static void ComposeHeader(
        IContainer container)
    {
        container
            .Row(row =>
            {
                row.RelativeItem()
                    .Column(column =>
                    {
                        column.Item()
                            .Text("DRIVEFLEET")
                            .Bold()
                            .FontSize(24);

                        column.Item()
                            .Text("Rent a Car")
                            .FontSize(10)
                            .FontColor(
                                Colors.Grey.Darken1);
                    });

                row.ConstantItem(210)
                    .AlignRight()
                    .Column(column =>
                    {
                        column.Item()
                            .AlignRight()
                            .Text("RESERVATION STATEMENT")
                            .Bold()
                            .FontSize(15);

                        column.Item()
                            .AlignRight()
                            .Text(
                                $"Generated on {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(8)
                            .FontColor(
                                Colors.Grey.Darken1);
                    });
            });
    }

    /// <summary>
    /// Builds one reservation section.
    /// </summary>
    private static void ComposeReservation(
        IContainer container,
        ReservationViewModel reservation)
    {
        container
            .Border(1)
            .BorderColor(
                Colors.Grey.Lighten2)
            .Padding(15)
            .Column(column =>
            {
                column.Spacing(12);

                column.Item()
                    .Element(section =>
                        ComposeReservationHeader(
                            section,
                            reservation));

                column.Item()
                    .LineHorizontal(1)
                    .LineColor(
                        Colors.Grey.Lighten2);

                column.Item()
                    .Element(section =>
                        ComposeCustomerInformation(
                            section,
                            reservation));

                column.Item()
                    .Element(section =>
                        ComposeItemsTable(
                            section,
                            reservation));

                column.Item()
                    .Element(section =>
                        ComposeReservationTotals(
                            section,
                            reservation));
            });
    }

    /// <summary>
    /// Builds the reservation identification area.
    /// </summary>
    private static void ComposeReservationHeader(
        IContainer container,
        ReservationViewModel reservation)
    {
        container
            .Row(row =>
            {
                row.RelativeItem()
                    .Column(column =>
                    {
                        column.Item()
                            .Text(
                                $"Reservation #{reservation.ReservationId}")
                            .Bold()
                            .FontSize(14);

                        column.Item()
                            .Text(
                                $"Status: {reservation.ReservationStatusName}")
                            .FontSize(9);
                    });

                row.RelativeItem()
                    .AlignRight()
                    .Column(column =>
                    {
                        column.Item()
                            .AlignRight()
                            .Text("Reservation date")
                            .FontSize(8)
                            .FontColor(
                                Colors.Grey.Darken1);

                        column.Item()
                            .AlignRight()
                            .Text(
                                reservation.CreatedAt
                                    .ToString(
                                        "dd/MM/yyyy HH:mm"));
                    });
            });
    }

    /// <summary>
    /// Builds the customer information area.
    /// </summary>
    private static void ComposeCustomerInformation(
        IContainer container,
        ReservationViewModel reservation)
    {
        container
            .Background(
                Colors.Grey.Lighten4)
            .Padding(10)
            .Column(column =>
            {
                column.Spacing(3);

                column.Item()
                    .Text("CUSTOMER")
                    .Bold()
                    .FontSize(9);

                column.Item()
                    .Text(
                        reservation.UserFullName);

                column.Item()
                    .Text(
                        reservation.UserEmail)
                    .FontSize(8)
                    .FontColor(
                        Colors.Grey.Darken1);
            });
    }

    /// <summary>
    /// Builds the reservation line-items table.
    /// </summary>
    private static void ComposeItemsTable(
        IContainer container,
        ReservationViewModel reservation)
    {
        container
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Element(HeaderCell)
                        .Text("Description");

                    header.Cell()
                        .Element(HeaderCell)
                        .Text("Period");

                    header.Cell()
                        .Element(HeaderCell)
                        .AlignCenter()
                        .Text("Qty.");

                    header.Cell()
                        .Element(HeaderCell)
                        .AlignRight()
                        .Text("Unit price");

                    header.Cell()
                        .Element(HeaderCell)
                        .AlignRight()
                        .Text("Amount");
                });

                foreach (var vehicle in reservation.Vehicles)
                {
                    table.Cell()
                        .Element(BodyCell)
                        .Column(column =>
                        {
                            column.Item()
                                .Text(
                                    $"{vehicle.VehicleBrand} " +
                                    $"{vehicle.VehicleModel}")
                                .SemiBold();

                            column.Item()
                                .Text(
                                    vehicle.LicensePlate)
                                .FontSize(8)
                                .FontColor(
                                    Colors.Grey.Darken1);
                        });

                    table.Cell()
                        .Element(BodyCell)
                        .Text(
                            $"{vehicle.StartDate:dd/MM/yyyy}\n" +
                            $"{vehicle.EndDate:dd/MM/yyyy}");

                    table.Cell()
                        .Element(BodyCell)
                        .AlignCenter()
                        .Text(
                            vehicle.RentalDays.ToString(
                                PortugueseCulture));

                    table.Cell()
                        .Element(BodyCell)
                        .AlignRight()
                        .Text(
                            FormatCurrency(
                                vehicle.DailyPrice));

                    table.Cell()
                        .Element(BodyCell)
                        .AlignRight()
                        .Text(
                            FormatCurrency(
                                vehicle.RentalValue));

                    foreach (var extra in vehicle.Extras)
                    {
                        table.Cell()
                            .Element(ExtraCell)
                            .Text(
                                $"Extra - {extra.Name}");

                        table.Cell()
                            .Element(ExtraCell)
                            .Text("-");

                        table.Cell()
                            .Element(ExtraCell)
                            .AlignCenter()
                            .Text(
                                extra.Quantity.ToString(
                                    PortugueseCulture));

                        table.Cell()
                            .Element(ExtraCell)
                            .AlignRight()
                            .Text(
                                FormatCurrency(
                                    extra.Price));

                        table.Cell()
                            .Element(ExtraCell)
                            .AlignRight()
                            .Text(
                                FormatCurrency(
                                    extra.Total));
                    }
                }
            });
    }

    /// <summary>
    /// Builds the reservation total section.
    /// </summary>
    private static void ComposeReservationTotals(
        IContainer container,
        ReservationViewModel reservation)
    {
        var rentalTotal =
            reservation.Vehicles.Sum(
                vehicle =>
                    vehicle.RentalValue);

        var extrasTotal =
            reservation.Vehicles.Sum(
                vehicle =>
                    vehicle.ExtrasValue);

        container
            .AlignRight()
            .Width(240)
            .Column(column =>
            {
                column.Spacing(5);

                column.Item()
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text("Vehicle rental:");

                        row.ConstantItem(90)
                            .AlignRight()
                            .Text(
                                FormatCurrency(
                                    rentalTotal));
                    });

                column.Item()
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text("Extras:");

                        row.ConstantItem(90)
                            .AlignRight()
                            .Text(
                                FormatCurrency(
                                    extrasTotal));
                    });

                column.Item()
                    .PaddingTop(5)
                    .LineHorizontal(1)
                    .LineColor(
                        Colors.Grey.Lighten2);

                column.Item()
                    .PaddingTop(5)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text("TOTAL")
                            .Bold()
                            .FontSize(12);

                        row.ConstantItem(90)
                            .AlignRight()
                            .Text(
                                FormatCurrency(
                                    reservation.TotalValue))
                            .Bold()
                            .FontSize(12);
                    });
            });
    }

    /// <summary>
    /// Builds the document footer.
    /// </summary>
    private static void ComposeFooter(
        IContainer container)
    {
        container
            .Column(column =>
            {
                column.Item()
                    .LineHorizontal(1)
                    .LineColor(
                        Colors.Grey.Lighten2);

                column.Item()
                    .PaddingTop(6)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text(
                                "DriveFleet - Reservation history document")
                            .FontSize(8)
                            .FontColor(
                                Colors.Grey.Darken1);

                        row.RelativeItem()
                            .AlignRight()
                            .Text(text =>
                            {
                                text.DefaultTextStyle(
                                    style =>
                                        style
                                            .FontSize(8)
                                            .FontColor(
                                                Colors.Grey.Darken1));

                                text.Span("Page ");

                                text.CurrentPageNumber();

                                text.Span(" of ");

                                text.TotalPages();
                            });
                    });
            });
    }

    /// <summary>
    /// Styles a table header cell.
    /// </summary>
    private static IContainer HeaderCell(
        IContainer container)
    {
        return container
            .Background(
                Colors.Grey.Lighten3)
            .BorderBottom(1)
            .BorderColor(
                Colors.Grey.Lighten1)
            .PaddingVertical(6)
            .PaddingHorizontal(4)
            .DefaultTextStyle(style =>
                style.SemiBold());
    }

    /// <summary>
    /// Styles a regular table cell.
    /// </summary>
    private static IContainer BodyCell(
        IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(
                Colors.Grey.Lighten3)
            .PaddingVertical(7)
            .PaddingHorizontal(4);
    }

    /// <summary>
    /// Styles an extra item table cell.
    /// </summary>
    private static IContainer ExtraCell(
        IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(
                Colors.Grey.Lighten4)
            .PaddingVertical(5)
            .PaddingHorizontal(4)
            .DefaultTextStyle(style =>
                style
                    .FontSize(8)
                    .FontColor(
                        Colors.Grey.Darken1));
    }

    /// <summary>
    /// Formats a monetary value using Portuguese
    /// currency formatting.
    /// </summary>
    private static string FormatCurrency(
        decimal value)
    {
        return value.ToString(
            "C",
            PortugueseCulture);
    }
}