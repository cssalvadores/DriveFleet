using System.Globalization;
using System.Net;
using DriveFleet.Mobile.Models.Reservations;
using DriveFleet.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace DriveFleet.Mobile.Pages;

/// <summary>
/// Displays the reservations scheduled
/// for the employee's current day.
/// </summary>
public partial class TodayReservationsPage : ContentPage
{
    private bool _isLoading;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="TodayReservationsPage"/> class.
    /// </summary>
    public TodayReservationsPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Refreshes today's reservations whenever
    /// the page becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        CurrentDateLabel.Text =
            DateTime.Now.ToString(
                "dddd, dd MMMM yyyy",
                CultureInfo.GetCultureInfo(
                    "en-GB"));

        await LoadReservationsAsync();
    }

    /// <summary>
    /// Loads today's reservations from
    /// the DriveFleet API.
    /// </summary>
    private async Task LoadReservationsAsync()
    {
        if (_isLoading)
        {
            return;
        }

        var services =
            IPlatformApplication
                .Current?
                .Services;

        var sessionService =
            services?
                .GetService<SessionService>();

        var reservationApiService =
            services?
                .GetService<ReservationApiService>();

        if (sessionService is null ||
            reservationApiService is null)
        {
            ShowError(
                "The reservation service is unavailable.");

            return;
        }

        var session =
            await sessionService.GetAsync();

        if (session is null)
        {
            await Shell.Current.GoToAsync(
                "//LoginPage");

            return;
        }

        try
        {
            _isLoading =
                true;

            ShowLoading();

            var result =
                await reservationApiService.GetByDateAsync(
                    session.AccessToken,
                    DateTime.Today);

            if (result.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                sessionService.Clear();

                await Shell.Current.GoToAsync(
                    "//LoginPage");

                return;
            }

            if (result.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                ShowError(
                    "You are not authorized to view these reservations.");

                return;
            }

            if (result.StatusCode !=
                HttpStatusCode.OK)
            {
                ShowError(
                    string.IsNullOrWhiteSpace(
                        result.Detail)
                        ? "Today's reservations could not be loaded."
                        : result.Detail);

                return;
            }

            var items =
                result.Reservations
                    .OrderBy(reservation =>
                        reservation.Vehicles
                            .Select(vehicle =>
                                vehicle.StartDate)
                            .DefaultIfEmpty(
                                DateTime.MaxValue)
                            .Min())
                    .Select(MapReservation)
                    .ToList();

            ReservationsCollectionView.ItemsSource =
                items;

            ReservationCountLabel.Text =
                items.Count.ToString(
                    CultureInfo.InvariantCulture);

            var hasReservations =
                items.Count > 0;

            ReservationsCollectionView.IsVisible =
                hasReservations;

            EmptyContainer.IsVisible =
                !hasReservations;

            ErrorContainer.IsVisible =
                false;
        }
        catch (HttpRequestException)
        {
            ShowError(
                "Unable to connect to the DriveFleet API.");
        }
        catch (TaskCanceledException)
        {
            ShowError(
                "The reservation request timed out.");
        }
        finally
        {
            _isLoading =
                false;

            LoadingContainer.IsVisible =
                false;
        }
    }

    /// <summary>
    /// Maps an API reservation to the model used
    /// by the daily operations interface.
    /// </summary>
    private static TodayReservationItemModel MapReservation(
        ReservationModel reservation)
    {
        var vehicles =
            reservation.Vehicles
                .OrderBy(vehicle =>
                    vehicle.StartDate)
                .ToList();

        var vehicleSummary =
            vehicles.Count == 0
                ? "No vehicle information"
                : string.Join(
                    ", ",
                    vehicles.Select(vehicle =>
                        $"{vehicle.VehicleBrand} " +
                        $"{vehicle.VehicleModel} " +
                        $"({vehicle.LicensePlate})"));

        var periodSummary =
            vehicles.Count == 0
                ? "No period information"
                : BuildPeriodSummary(
                    vehicles);

        return new TodayReservationItemModel
        {
            ReservationId =
                reservation.ReservationId,

            CustomerName =
                reservation.UserFullName,

            CustomerEmail =
                reservation.UserEmail,

            ReservationStatusName =
                reservation.ReservationStatusName,

            TotalValue =
                reservation.TotalValue,

            VehicleCount =
                vehicles.Count,

            VehicleSummary =
                vehicleSummary,

            PeriodSummary =
                periodSummary
        };
    }

    /// <summary>
    /// Builds a readable rental period
    /// from the vehicles in a reservation.
    /// </summary>
    private static string BuildPeriodSummary(
        IReadOnlyCollection<ReservationVehicleModel> vehicles)
    {
        var start =
            vehicles.Min(vehicle =>
                vehicle.StartDate);

        var end =
            vehicles.Max(vehicle =>
                vehicle.EndDate);

        if (start.Date ==
            end.Date)
        {
            return
                $"{start:HH:mm} → {end:HH:mm}";
        }

        return
            $"{start:dd MMM HH:mm} → " +
            $"{end:dd MMM HH:mm}";
    }

    /// <summary>
    /// Returns to the employee home page.
    /// </summary>
    private async void OnBackClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//EmployeeHomePage");
    }

    /// <summary>
    /// Retries loading today's reservations.
    /// </summary>
    private async void OnRetryClicked(
        object sender,
        EventArgs e)
    {
        await LoadReservationsAsync();
    }

    /// <summary>
    /// Displays the reservation loading state.
    /// </summary>
    private void ShowLoading()
    {
        LoadingContainer.IsVisible =
            true;

        ErrorContainer.IsVisible =
            false;

        EmptyContainer.IsVisible =
            false;

        ReservationsCollectionView.IsVisible =
            false;

        ReservationCountLabel.Text =
            "...";
    }

    /// <summary>
    /// Displays a reservation loading error.
    /// </summary>
    private void ShowError(
        string message)
    {
        LoadingContainer.IsVisible =
            false;

        ReservationsCollectionView.IsVisible =
            false;

        EmptyContainer.IsVisible =
            false;

        ErrorMessageLabel.Text =
            message;

        ErrorContainer.IsVisible =
            true;

        ReservationCountLabel.Text =
            "!";
    }
}