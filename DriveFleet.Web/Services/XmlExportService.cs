using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DriveFleet.Web.Services;

/// <summary>
/// Generates structured XML exports for DriveFleet data.
/// </summary>
public class XmlExportService
{
    private const string CurrencyCode = "EUR";

    /// <summary>
    /// Generates an XML document containing the current vehicle inventory.
    /// </summary>
    /// <param name="vehicles">
    /// Vehicles included in the export.
    /// </param>
    /// <returns>
    /// UTF-8 encoded XML document bytes.
    /// </returns>
    public byte[] GenerateVehiclesExport(
        IReadOnlyList<VehicleApiModel> vehicles)
    {
        var document =
            new XDocument(
                new XDeclaration(
                    "1.0",
                    "utf-8",
                    null),
                new XElement(
                    "DriveFleetExport",
                    new XAttribute(
                        "type",
                        "Vehicles"),
                    new XAttribute(
                        "generatedAt",
                        FormatDateTime(
                            DateTime.UtcNow)),
                    new XElement(
                        "Vehicles",
                        new XAttribute(
                            "count",
                            vehicles.Count),
                        vehicles.Select(
                            CreateVehicleElement))));

        return GenerateXmlBytes(
            document);
    }

    /// <summary>
    /// Generates an XML document containing reservation information.
    /// </summary>
    /// <param name="reservations">
    /// Reservations included in the export.
    /// </param>
    /// <param name="fromDate">
    /// Optional first date used to filter the reservations.
    /// </param>
    /// <param name="toDate">
    /// Optional last date used to filter the reservations.
    /// </param>
    /// <returns>
    /// UTF-8 encoded XML document bytes.
    /// </returns>
    public byte[] GenerateReservationsExport(
        IReadOnlyList<ReservationApiModel> reservations,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var document =
            new XDocument(
                new XDeclaration(
                    "1.0",
                    "utf-8",
                    null),
                new XElement(
                    "DriveFleetExport",
                    new XAttribute(
                        "type",
                        "Reservations"),
                    new XAttribute(
                        "generatedAt",
                        FormatDateTime(
                            DateTime.UtcNow)),
                    CreatePeriodElement(
                        fromDate,
                        toDate),
                    new XElement(
                        "Reservations",
                        new XAttribute(
                            "count",
                            reservations.Count),
                        reservations.Select(
                            CreateReservationElement))));

        return GenerateXmlBytes(
            document);
    }

    /// <summary>
    /// Creates one vehicle XML element.
    /// </summary>
    private static XElement CreateVehicleElement(
        VehicleApiModel vehicle)
    {
        return new XElement(
            "Vehicle",
            new XAttribute(
                "id",
                vehicle.VehicleId),

            new XElement(
                "Brand",
                vehicle.Brand),

            new XElement(
                "Model",
                vehicle.Model),

            new XElement(
                "Year",
                vehicle.Year),

            new XElement(
                "LicensePlate",
                vehicle.LicensePlate),

            new XElement(
                "Seats",
                vehicle.Seats),

            new XElement(
                "DailyPrice",
                new XAttribute(
                    "currency",
                    CurrencyCode),
                FormatDecimal(
                    vehicle.DailyPrice)),

            new XElement(
                "Category",
                new XAttribute(
                    "id",
                    vehicle.CategoryId),
                vehicle.CategoryName),

            new XElement(
                "Status",
                new XAttribute(
                    "id",
                    vehicle.VehicleStatusId),
                vehicle.VehicleStatusName),

            CreateOptionalElement(
                "Description",
                vehicle.Description),

            new XElement(
                "CreatedAt",
                FormatDateTime(
                    vehicle.CreatedAt)),

            CreateOptionalDateTimeElement(
                "UpdatedAt",
                vehicle.UpdatedAt),

            new XElement(
                "Photos",
                new XAttribute(
                    "count",
                    vehicle.Photos.Count),
                vehicle.Photos
                    .OrderBy(photo =>
                        photo.DisplayOrder)
                    .Select(photo =>
                        new XElement(
                            "Photo",
                            new XAttribute(
                                "id",
                                photo.VehiclePhotoId),
                            new XElement(
                                "FilePath",
                                photo.FilePath),
                            new XElement(
                                "DisplayOrder",
                                photo.DisplayOrder))))
        );
    }

    /// <summary>
    /// Creates one reservation XML element.
    /// </summary>
    private static XElement CreateReservationElement(
        ReservationApiModel reservation)
    {
        return new XElement(
            "Reservation",
            new XAttribute(
                "id",
                reservation.ReservationId),

            new XElement(
                "Customer",
                new XAttribute(
                    "id",
                    reservation.UserId),

                new XElement(
                    "FullName",
                    reservation.UserFullName),

                new XElement(
                    "Email",
                    reservation.UserEmail)),

            new XElement(
                "Status",
                new XAttribute(
                    "id",
                    reservation.ReservationStatusId),
                reservation.ReservationStatusName),

            new XElement(
                "TotalValue",
                new XAttribute(
                    "currency",
                    CurrencyCode),
                FormatDecimal(
                    reservation.TotalValue)),

            new XElement(
                "CreatedAt",
                FormatDateTime(
                    reservation.CreatedAt)),

            CreateOptionalDateTimeElement(
                "UpdatedAt",
                reservation.UpdatedAt),

            new XElement(
                "Vehicles",
                new XAttribute(
                    "count",
                    reservation.Vehicles.Count),
                reservation.Vehicles.Select(
                    CreateReservationVehicleElement))
        );
    }

    /// <summary>
    /// Creates one reserved vehicle XML element.
    /// </summary>
    private static XElement CreateReservationVehicleElement(
        ReservationVehicleApiModel vehicle)
    {
        return new XElement(
            "ReservationVehicle",
            new XAttribute(
                "id",
                vehicle.ReservationVehicleId),

            new XAttribute(
                "vehicleId",
                vehicle.VehicleId),

            new XElement(
                "Brand",
                vehicle.VehicleBrand),

            new XElement(
                "Model",
                vehicle.VehicleModel),

            new XElement(
                "LicensePlate",
                vehicle.LicensePlate),

            new XElement(
                "StartDate",
                FormatDateTime(
                    vehicle.StartDate)),

            new XElement(
                "EndDate",
                FormatDateTime(
                    vehicle.EndDate)),

            new XElement(
                "RentalDays",
                vehicle.RentalDays),

            new XElement(
                "DailyPrice",
                new XAttribute(
                    "currency",
                    CurrencyCode),
                FormatDecimal(
                    vehicle.DailyPrice)),

            new XElement(
                "RentalValue",
                new XAttribute(
                    "currency",
                    CurrencyCode),
                FormatDecimal(
                    vehicle.RentalValue)),

            new XElement(
                "ExtrasValue",
                new XAttribute(
                    "currency",
                    CurrencyCode),
                FormatDecimal(
                    vehicle.ExtrasValue)),

            new XElement(
                "TotalValue",
                new XAttribute(
                    "currency",
                    CurrencyCode),
                FormatDecimal(
                    vehicle.TotalValue)),

            new XElement(
                "Extras",
                new XAttribute(
                    "count",
                    vehicle.Extras.Count),
                vehicle.Extras.Select(
                    CreateExtraElement))
        );
    }

    /// <summary>
    /// Creates one reservation extra XML element.
    /// </summary>
    private static XElement CreateExtraElement(
        ReservationExtraApiModel extra)
    {
        return new XElement(
            "Extra",
            new XAttribute(
                "id",
                extra.ExtraId),

            new XElement(
                "Name",
                extra.Name),

            new XElement(
                "Quantity",
                extra.Quantity),

            new XElement(
                "UnitPrice",
                new XAttribute(
                    "currency",
                    CurrencyCode),
                FormatDecimal(
                    extra.Price)),

            new XElement(
                "Total",
                new XAttribute(
                    "currency",
                    CurrencyCode),
                FormatDecimal(
                    extra.Total))
        );
    }

    /// <summary>
    /// Creates the optional report filtering period element.
    /// </summary>
    private static XElement CreatePeriodElement(
        DateTime? fromDate,
        DateTime? toDate)
    {
        var element =
            new XElement(
                "FilterPeriod");

        if (fromDate.HasValue)
        {
            element.Add(
                new XElement(
                    "From",
                    fromDate.Value
                        .ToString(
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture)));
        }

        if (toDate.HasValue)
        {
            element.Add(
                new XElement(
                    "To",
                    toDate.Value
                        .ToString(
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture)));
        }

        return element;
    }

    /// <summary>
    /// Creates an element only when a text value is available.
    /// </summary>
    private static XElement CreateOptionalElement(
        string elementName,
        string? value)
    {
        return new XElement(
            elementName,
            string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value);
    }

    /// <summary>
    /// Creates an optional date time element.
    /// </summary>
    private static XElement CreateOptionalDateTimeElement(
        string elementName,
        DateTime? value)
    {
        return new XElement(
            elementName,
            value.HasValue
                ? FormatDateTime(
                    value.Value)
                : string.Empty);
    }

    /// <summary>
    /// Formats a decimal value using an integration-safe representation.
    /// </summary>
    private static string FormatDecimal(
        decimal value)
    {
        return value.ToString(
            "0.00",
            CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a date time value using the ISO 8601 standard.
    /// </summary>
    private static string FormatDateTime(
        DateTime value)
    {
        return value.ToString(
            "O",
            CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Serializes an XML document using UTF-8 without a byte-order mark.
    /// </summary>
    private static byte[] GenerateXmlBytes(
        XDocument document)
    {
        using var memoryStream =
            new MemoryStream();

        var settings =
            new XmlWriterSettings
            {
                Encoding =
                    new UTF8Encoding(
                        encoderShouldEmitUTF8Identifier:
                            false),

                Indent = true,

                OmitXmlDeclaration = false
            };

        using (var writer =
               XmlWriter.Create(
                   memoryStream,
                   settings))
        {
            document.Save(
                writer);
        }

        return memoryStream.ToArray();
    }
}
